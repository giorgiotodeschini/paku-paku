namespace PakuPaku.Domain;


public sealed class GameState
{
    public int CurrentLevel { get; set; } = 1;
    public int TotalDotsRemaining { get; set; }
    public int[,] Maze { get; set; } = (int[,])MazeData.OriginalMap.Clone();

    public GameState()
    {
        ReadyTimerFrames = 120;
        LoadInitialMap();
    }
    public LevelDefinition CurrentLevelConfig => LevelProgression.GetConfigForLevel(CurrentLevel);

    public void LoadInitialMap()
    {
        // Deep cloning of the original matrix to reset it at every level.
        Maze = (int[,])MazeData.OriginalMap.Clone();
        TotalDotsRemaining = Maze.Cast<int>().Count(x => x == 2);
    }
    
    public Player Player { get; } = new(14, 26);
    public List<Ghost> Ghosts { get; } =
    [
        new Ghost(GhostType.Blinky, 14, 16), // Red
        new Ghost(GhostType.Pinky, 14, 18),  // Pink
        new Ghost(GhostType.Inky, 12, 18),   // Cyan
        new Ghost(GhostType.Clyde, 16, 18)   // Orange
    ];

    public int FreezeFrames { get; set; } = 0; // If > 0, Pac-man is unlocked (He has just eaten)
    public int Score { get; set; } = 0;

    public int Lives { get; set; } = 3;
    public bool IsGameOver { get; set; } = false;
    public bool ShouldResetRound { get; set; } = false;
    
    public int PacmanFreezeFrames { get; set; } = 0;   // Freezes only for Pac-man when eats dots
    public int GlobalFreezeFrames { get; set; } = 0;   // Freezes the whole screen when it eats a ghost
    public int FrightenedTimerFrames { get; set; } = 0; // Remaining duration of the ghost scare effect
    public int ReadyTimerFrames { get; set; } = 0;
    public int GhostEatMultiplier { get; set; } = 1;   // Track whether you've eaten 1, 2, 3, or 4 ghosts in a row.

    public void ResetRoundPositions()
    {
        Player.State.TileX = 14;
        Player.State.TileY = 26;
        Player.State.SubPixelX = 0;
        Player.State.SubPixelY = 0;
        Player.State.CurrentDirection = Direction.None;
        Player.State.NextDirection = Direction.None;

        // Reset the gosts 
        foreach (var ghost in Ghosts)
        {
            ghost.CurrentMode = GhostMode.Scatter;
            ghost.State.SubPixelX = 0;
            ghost.State.SubPixelY = 0;
            ghost.State.CurrentDirection = Direction.None;
            ghost.State.NextDirection = Direction.None;
            ghost.State.TileX = ghost.Type switch
            {
                GhostType.Blinky or GhostType.Pinky  => 14,
                GhostType.Inky => 12,
                GhostType.Clyde => 16,
                _ => 0
            };
            ghost.State.TileY = ghost.Type switch
            {
                GhostType.Blinky  => 16,
                GhostType.Pinky or GhostType.Inky or GhostType.Clyde => 18,
                _ => 0
            };
            
            // Set the speed base reading from the new level coinfig
            ghost.State.Speed = CurrentLevelConfig.GhostSpeedNormal;
        }
    }
}