namespace PakuPaku.Domain;

public class EntityState
{
    public int TileX { get; set; }
    public int TileY { get; set; }
    public int SubPixelX { get; set; }
    public int SubPixelY { get; set; }
    public Direction CurrentDirection { get; set; }
    public Direction NextDirection { get; set; } 
    public int Speed { get; set; } = 1; 
}