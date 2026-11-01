namespace PakuPaku.Domain;

public class Player
{
    public EntityState State { get; } = new();

    public Player(int startTileX, int startTileY)
    {
        State.TileX = startTileX;
        State.TileY = startTileY;
    }
}