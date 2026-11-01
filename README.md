# PakuPaku - a PacMan clone

PakuPaku is a PacMan clone written in C# on .NET 10, using [Raylib-cs](https://github.com/chrisdill/raylib-cs) for window, input, rendering and audio. The project is primarily a study of how to structure a small real-time game with **Vertical Slice Architecture** and the **SOLID** principles, without a framework and without a test suite.

The gameplay mimics the original arcade game: a 28x36 tile maze, dots and energizers, four ghosts with distinct chase personalities, Chase/Scatter/Frightened/Eaten modes, lives, levels with progressively shorter fright timers, and the classic sound effects.

## Requirements

- .NET 10 SDK
- A platform supported by Raylib-cs (Windows, macOS, Linux)

## Build and run

From the repository root:

```bash
dotnet run --project PakuPaku
```

Controls: **arrow keys** change the direction of Pac-Man. The direction is applied at the next tile center.

## Architecture

### Vertical slices

The code is organized by **feature**, not by technical layer. Each gameplay behaviour lives in its own folder under `PakuPaku/Features/`, and contains everything it needs: its contract, its implementation, and the registration code for the DI container.

```
PakuPaku/
├── Program.cs                      Composition root (DI registration)
├── Domain/                         Shared domain model (no behaviour beyond data/rules)
│   ├── GameState.cs                Single mutable source of truth for the running game
│   ├── Player.cs / Ghost.cs        Actors, each wrapping an EntityState
│   ├── EntityState.cs              Tile + sub-pixel position, direction, speed
│   ├── Direction.cs                Direction enum
│   ├── MazeData.cs                 Original 28x36 maze matrix
│   ├── LevelDefinition.cs          Per-level tuning values (record)
│   └── LevelProgression.cs         Maps a level number to its LevelDefinition
├── Common/                         Cross-cutting infrastructure and the shared engine
│   ├── GameEngine.cs               Game loop: orchestrates the slices each frame
│   ├── IGameEngine.cs
│   ├── MovementEngine.cs           Tile/sub-pixel movement and wall checks (used by the Movement slice)
│   ├── Consts.cs                   Scale, tile size, sprite size
│   ├── Window/                     IGameWindow + RaylibWindow
│   ├── Input/                      IInputSystem + RaylibInputSystem
│   ├── Audio/                      IAudioService + RaylibAudioService
│   └── Graphics/                   IRenderingContext + RaylibRenderingContext
└── Features/                       One folder per vertical slice
    ├── Movement/
    │   ├── IMoveAndEatCommand.cs / MoveAndEatCommand.cs
    │   ├── IMoveGhostsCommand.cs / MoveGhostsCommand.cs
    │   └── MovementSliceExtensions.cs
    ├── GhostAI/
    │   ├── IProcessGhostAISystem.cs / ProcessGhostAISystem.cs
    │   └── GhostAISliceExtensions.cs
    ├── CollisionAndEating/
    │   ├── IProcessCollisionsCommand.cs / ProcessCollisionsCommand.cs
    │   └── CollisionAndEatingSliceExtensions.cs
    ├── LevelProgression/
    │   ├── ICheckLevelCompleteCommand.cs / CheckLevelCompleteCommand.cs
    │   └── LevelProgressionSliceExtensions.cs
    ├── Rendering/
    │   ├── IDrawFrameCommand.cs / DrawFrameCommand.cs
    │   └── RenderingSliceExtensions.cs
    └── Audio/
        ├── IAmbientAudioSystem.cs / AmbientAudioSystem.cs
        └── AudioSliceExtensions.cs
```

Each slice follows the same anatomy:

1. **A contract** (`IXxxCommand` or `IXxxSystem`) with a single `Execute()` method. This is the only public surface the game loop depends on.
2. **An implementation** (`XxxCommand` / `XxxSystem`) that receives its dependencies through the primary constructor.
3. **A registration extension** (`XxxSliceExtensions`) exposing `AddXxxSlice(this IServiceCollection)`, which is the only place where the slice is wired into the container.

`Program.cs` composes the application by calling these extensions:

```csharp
var serviceProvider = new ServiceCollection()
    .AddSingleton<GameState>()                 // Global state
    .AddSingleton<IGameWindow, RaylibWindow>()
    .AddSingleton<IRenderingContext, RaylibRenderingContext>()
    .AddSingleton<IInputSystem, RaylibInputSystem>()
    .AddSingleton<IAudioService, RaylibAudioService>()
    .AddSingleton<IGameEngine, GameEngine>()
    .AddSingleton<MovementEngine>()
    .AddGhostAISlice()
    .AddMovementSlice()
    .AddRenderingSlice()
    .AddAudioSlice()
    .AddCollisionAndEatingSlice()
    .AddLevelProgressionSlice()
    .BuildServiceProvider();
```

Adding a new feature means adding a folder under `Features/`, its contract and implementation, and one `AddXxxSlice()` call in `Program.cs`. No existing slice needs to change.

### Slice isolation

Slices never reference each other. Their only shared dependencies are:

- `Domain/` (the model, read and written through `GameState`);
- `Common/` (interfaces such as `IAudioService`, `IRenderingContext`, and the `MovementEngine` helper);
- `Common/Consts.cs`.

For example, `ProcessCollisionsCommand` and `CheckLevelCompleteCommand` both use `IAudioService` from `Common/Audio`, but neither depends on any class in another feature folder.

### Frame pipeline

`GameEngine.Run()` (`Common/GameEngine.cs`) is the only class that knows the order in which slices execute. Each iteration of the loop performs:

1. `IInputSystem.Execute()`: reads keyboard input and sets `NextDirection` on Pac-Man.
2. Global freeze check (`GameState.GlobalFreezeFrames`), used after a ghost is eaten or a life is lost.
3. Fright timer management: when `FrightenedTimerFrames` reaches zero, `ResetGhostsToNormalMode()` restores ghosts to `Chase`.
4. `IProcessGhostAISystem.Execute()`: chooses the next direction of each ghost at tile centers.
5. `IMoveAndEatCommand.Execute()` and `IMoveGhostsCommand.Execute()`: move the actors through `MovementEngine`.
6. `IProcessCollisionsCommand.Execute()`: dot/energizer consumption, ghost-versus-Pac-Man collisions, scoring, deaths.
7. `ICheckLevelCompleteCommand.Execute()`: advances the level when no dots remain.
8. `IAmbientAudioSystem.Execute()`: switches between the normal and frightened siren.
9. `IDrawFrameCommand.Execute()`: draws the frame between `BeginDrawing()` and `EndDrawing()`.
10. If `GameState.ShouldResetRound` is set, the round is reset (ghosts and Pac-Man return to their starting tiles).

The engine depends only on interfaces. It does contain a small amount of gameplay glue (for example `ResetGhostsToNormalMode()`), which is a compromise discussed below.

### Movement model

Positions are stored as a logical tile (`TileX`, `TileY`) plus a sub-pixel offset (`SubPixelX`, `SubPixelY`) in `EntityState`. A tile is 8 sub-pixels wide, and the game renders at `Consts.Scale = 3`. Decisions (turning, AI targeting, collisions on dots) happen only when an entity is exactly at a tile center, which mirrors the grid-based movement of the original game. `MovementEngine.UpdateEntity` applies a pending `NextDirection` at the tile center, stops the entity when it hits a wall, and advances the sub-pixel position by `Speed` each frame. It also handles the horizontal tunnel wrap-around.

## SOLID principles in the code

**Single Responsibility.** Each slice has one reason to change. `MoveAndEatCommand` only decides whether Pac-Man is frozen before delegating to `MovementEngine`; `MoveGhostsCommand` only iterates the ghosts; `DrawFrameCommand` only renders; `AmbientAudioSystem` only selects the siren. `RaylibRenderingContext`, `RaylibAudioService`, `RaylibWindow` and `RaylibInputSystem` each wrap exactly one Raylib concern. `MovementEngine` owns the geometry of movement and nothing else, and `LevelProgression` owns the level-to-tuning mapping.

**Open/Closed.** New features are added by creating a new slice, not by editing existing ones. `GameEngine` depends on the `IXxx` interfaces, so adding behaviour requires a new constructor parameter and a new call in `Run()`, but no change to the slices. `LevelProgression.GetConfigForLevel` returns the configuration of the highest defined level not greater than the current one, so new difficulty steps are added by inserting entries in its dictionary. Rendering and audio are also open for extension: replacing `RaylibRenderingContext` or `RaylibAudioService` requires only a different registration in `Program.cs`.

**Liskov Substitution.** All implementations honour the contracts defined by their interfaces. `IAudioService` implementations can be swapped without the callers noticing, because callers only use the string keys documented by the interface, and missing keys are handled gracefully (`TryGetValue`). Each `IXxxCommand` / `IXxxSystem` has a single `Execute()` method with no hidden preconditions, so any implementation can be used in the loop.

**Interface Segregation.** The interfaces are deliberately small. `IGameWindow` has three methods, `IAudioService` has five, and every slice contract exposes exactly one `Execute()`. `GameEngine` receives only the slice contracts it calls, not the full set of implementations or any Raylib type.

**Dependency Inversion.** High-level policy depends on abstractions. `GameEngine` depends on `IGameWindow`, `IRenderingContext`, `IInputSystem`, `IAudioService` and the `IXxx` slice contracts, never on the Raylib implementations. The concrete types are bound only in `Program.cs`, which is the composition root. Slices receive `GameState`, `MovementEngine` and service interfaces through constructor injection, using C# primary constructors.

### Trade-offs and honest notes

The project respects the principles above, but a few choices are deliberate compromises:

- **`GameState` is a shared mutable object.** It is registered as a singleton and read and written by every slice. This is what makes the frame pipeline simple and fast, but it means slices are coupled through data rather than through messages or events. Dependency Inversion is respected at the service level, not at the state level.
- **`ProcessCollisionsCommand` has more than one responsibility.** It handles dot and energizer consumption, ghost collisions, scoring, fright triggering and player death in a single class (about 160 lines). This is the slice that would benefit most from being split into smaller commands, for example a `ConsumeDotsCommand` and a `ResolveGhostCollisionsCommand`.
- **`ProcessGhostAISystem` uses `switch` statements over `GhostType` and `GhostMode`.** Adding a new ghost personality means editing this class, which is a partial violation of Open/Closed. A strategy per ghost type (`IGhostTargetingStrategy`) would remove this coupling.
- **Movement and AI share geometry.** `GhostAI` has its own neighbour and direction logic, and `MovementEngine` has its own wall checks. Both use the same maze data but not the same helper.
- **`MovementEngine` is a concrete class in `Common/`.** It is used by the Movement slice (`MoveAndEatCommand`, `MoveGhostsCommand`) and is registered directly in `Program.cs`. It is not behind an interface, which is a pragmatic choice for a single implementation, but it could be moved into `Features/Movement/` since only that slice uses it.
- **`MazeData.OriginalMap` is hardcoded.** The maze is a constant in `Domain/MazeData.cs`. Supporting different maps would require a loader and a different dependency on it.

## Known limitations

- There are no automated tests. The architecture (interfaces plus constructor injection) would make unit testing straightforward, particularly for `MovementEngine`, `ProcessGhostAISystem` and `ProcessCollisionsCommand`.
- `GameEngine.Run()` uses `Thread.Sleep(3000)` to pause after a death. This blocks the window and the input loop during the pause.
- The HUD shows a hardcoded `HIGH SCORE` value of `0`.
- `RaylibAudioService.PlayLoopingSound` does not loop: it behaves like `PlaySound` and only restarts the sound when it is not playing.
- The maze width (28) is repeated as a literal in `MovementEngine.CanMoveInDirection` and `ProcessGhostAISystem.CalculateNextDirection` rather than derived from `MazeData`.

## Assets

The sprites in `PakuPaku/Assets/spritesheet.png` and the sound effects in `PakuPaku/Assets/Audio/` were created for this project: the sprites are pixel art generated by a Python script, and the sound effects are synthesized procedurally. They are covered by the same license as the code.

"Pac-Man" is a trademark of Bandai Namco. This project is an independent, non-commercial clone and is not affiliated with or endorsed by Bandai Namco.

## License

This project is licensed under the MIT License. See [LICENSE](LICENSE) for the full text.

Copyright (c) 2026 Giorgio Todeschini
