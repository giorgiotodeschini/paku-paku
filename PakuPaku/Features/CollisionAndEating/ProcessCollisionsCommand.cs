using PakuPaku.Common;
using PakuPaku.Common.Audio;
using PakuPaku.Domain;

namespace PakuPaku.Features.CollisionAndEating;

public sealed class ProcessCollisionsCommand(
    GameState gameState,
    IAudioService audioService) 
    : IProcessCollisionsCommand
{
    private readonly GameState _gameState = gameState;
    private readonly IAudioService _audioService = audioService;
    
    public void Execute()
    {
        if (_gameState.IsGameOver) return;

        // 1. COLLISION WITH MAP ELEMENTS (Dots / Pellets)
        // Occurs ONLY when Pac-Man is perfectly aligned with the center of the logical tile
        if (_gameState.Player.State.SubPixelX == 0 && _gameState.Player.State.SubPixelY == 0)
        {
            ProcessMapCollisions();
        }

        // 2. COLLISION WITH GHOSTS (Entity vs. Entity)
        // Occurs every frame (60Hz) based on continuous sub-pixel distance
        ProcessGhostCollisions();
    }

    private void ProcessMapCollisions()
    {
        int pX = _gameState.Player.State.TileX;
        int pY = _gameState.Player.State.TileY;

        // Matrix boundary safety
        if (pY < 0 || pY >= _gameState.Maze.GetLength(0) || pX < 0 || pX >= _gameState.Maze.GetLength(1)) return;

        int tileContent = _gameState.Maze[pY, pX];

        if (tileContent == 2) // 2 = Dot
        {
            _gameState.Maze[pY, pX] = 0; // Empty the tile
            _gameState.Score += 10;
            _gameState.PacmanFreezeFrames = 1; // Freeze the Player for 1 logic frames.
            _gameState.TotalDotsRemaining--;

            _audioService.PlaySound("eat_dot");
        }
        else if (tileContent == 3) // 3 = Energizer
        {
            _gameState.Maze[pY, pX] = 0;
            _gameState.Score += 50;
            _gameState.PacmanFreezeFrames = 3; // Freeze the Player for 3 logic frames.
            _gameState.TotalDotsRemaining--;

            // Trigger Fright Mode for all the ghosts that haven't been eaten yet.
            TriggerFrightenedMode();
        }
    }

    private void ProcessGhostCollisions()
    {
        // Valculate Player's absolute position in total sub-pixels from the origin (0,0).
        int pacmanAbsX = _gameState.Player.State.TileX * Consts.BaseSize + _gameState.Player.State.SubPixelX;
        int pacmanAbsY = _gameState.Player.State.TileY * Consts.BaseSize + _gameState.Player.State.SubPixelY;

        // Rear tolerance distance: ~4 original sub-pixels (half a tile)
        int collisionThreshold = 4; 

        foreach (var ghost in _gameState.Ghosts)
        {
            // Ignore ghosts that have already been eaten and are returning to base as eyes.
            if (ghost.CurrentMode == GhostMode.Eaten) continue;

            int ghostAbsX = ghost.State.TileX * Consts.BaseSize + ghost.State.SubPixelX;
            int ghostAbsY = ghost.State.TileY * Consts.BaseSize + ghost.State.SubPixelY;

            // Calculation of Manhattan or Euclidean distance in sub-pixels. 
            // We use the squared distance to avoid the computationally expensive Math.Sqrt operation at 60Hz.
            int deltaX = pacmanAbsX - ghostAbsX;
            int deltaY = pacmanAbsY - ghostAbsY;
            int distanceSquared = (deltaX * deltaX) + (deltaY * deltaY);

            if (distanceSquared < (collisionThreshold * collisionThreshold))
            {
                // COLLISION OCCURRED! Fate depends on the state of the Phantom.
                if (ghost.CurrentMode == GhostMode.Frightened)
                {
                    // The player eats the ghost
                    ghost.CurrentMode = GhostMode.Eaten;
                    ghost.State.Speed = ghost.CurrentMode switch
                    {
                        GhostMode.Eaten => 3, // Gli occhi vanno sempre velocissimi
                        GhostMode.Frightened => _gameState.CurrentLevelConfig.GhostSpeedFrightened,
                        _ => _gameState.CurrentLevelConfig.GhostSpeedNormal
                    };
                    
                    _gameState.Score += _gameState.GhostEatMultiplier * 200;
                    _gameState.GhostEatMultiplier *= 2; // Doubles the bonus for the next ghost (200, 400, 800, 1600)
                    
                    // Applies a macroscopic global freeze (approx. 30-frame stop) for the original dramatic effect
                    _gameState.GlobalFreezeFrames = 30; 
                }
                else
                {
                    ProcessPlayerDeath();
                    break;
                }
            }
        }
    }

    private void TriggerFrightenedMode()
    {
        _gameState.GhostEatMultiplier = 1; // Reset score multiplier
        
        foreach (var ghost in _gameState.Ghosts)
        {
            if (ghost.CurrentMode == GhostMode.Chase || ghost.CurrentMode == GhostMode.Scatter)
            {
                ghost.CurrentMode = GhostMode.Frightened;
                
                // Classic rule: when ghosts get scared, they IMMEDIATELY reverse direction. 
                // This is the only exception where they can do so voluntarily.
                ghost.State.NextDirection = GetOppositeDirection(ghost.State.CurrentDirection);
            }
        }
        
        // Set the fright duration timer in the GameState (eg. 420 frame = 7 seconds to 60Hz)
        _gameState.FrightenedTimerFrames = 420;
    }

    private void ProcessPlayerDeath()
    {
        _gameState.Lives--;

        _audioService.PlaySound("player_death");

        if (_gameState.Lives < 0)
        {
            _gameState.IsGameOver = true;
        }
        else
        {
            // Reset ghost and player to the initial coordinates
            _gameState.ShouldResetRound = true;
            _gameState.GlobalFreezeFrames = 180;
        }
    }

    private static Direction GetOppositeDirection(Direction dir) => dir switch
    {
        Direction.Up => Direction.Down,
        Direction.Down => Direction.Up,
        Direction.Left => Direction.Right,
        Direction.Right => Direction.Left,
        _ => Direction.None
    };
}