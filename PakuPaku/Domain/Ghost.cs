namespace PakuPaku.Domain;

public enum GhostType { Blinky, Pinky, Inky, Clyde }
public enum GhostMode { Chase, Scatter, Frightened, Eaten }

public class Ghost
{
    public GhostType Type { get; init; }
    public EntityState State { get; } = new();
    public GhostMode CurrentMode { get; set; } = GhostMode.Chase; 
    
    public int TargetTileX { get; set; }
    public int TargetTileY { get; set; }

    public Ghost(GhostType type, int startTileX, int startTileY)
    {
        Type = type;
        State.TileX = startTileX;
        State.TileY = startTileY;
    }
}