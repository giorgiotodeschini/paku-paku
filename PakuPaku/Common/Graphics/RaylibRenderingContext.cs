using System.Numerics;
using Raylib_cs;

namespace PakuPaku.Common.Graphics;

public sealed class RaylibRenderingContext : IRenderingContext
{
    private static readonly int[] WallAtlasFrameByMask =
    [
        15, 11, 12, 3,
        13, 1, 4, 7,
        14, 6, 2, 8,
        5, 9, 10, 0
    ];

    private Texture2D _mainSpritesheet;
    private readonly Dictionary<string, SpriteMetadata> _spriteRegistry = new();

    public void Initialize()
    {
        // Load the image in the GPU memory with Raylib
        string spritesheetPath = Path.Combine(AppContext.BaseDirectory, "Assets", "spritesheet.png");
        _mainSpritesheet = Raylib.LoadTexture(spritesheetPath);

        // 2. Registriamo la mappatura geometrica degli sprite presenti nella nostra immagine.
        // Parametri: Y di riga, altezza e larghezza del singolo frame dello sprite.
        _spriteRegistry.Add("pacman",       new SpriteMetadata(SourceY: 0,  Width: 16, Height: 16));
        _spriteRegistry.Add("ghost_blinky", new SpriteMetadata(SourceY: 16, Width: 16, Height: 16));
        _spriteRegistry.Add("ghost_pinky",  new SpriteMetadata(SourceY: 32, Width: 16, Height: 16));
        _spriteRegistry.Add("ghost_inky",   new SpriteMetadata(SourceY: 48, Width: 16, Height: 16));
        _spriteRegistry.Add("ghost_clyde",  new SpriteMetadata(SourceY: 64, Width: 16, Height: 16));
        _spriteRegistry.Add("ghost_scared", new SpriteMetadata(SourceY: 80, Width: 16, Height: 16));
        _spriteRegistry.Add("ghost_eyes",   new SpriteMetadata(SourceY: 96, Width: 16, Height: 16));
        
        // Mappatura per i tile del labirinto (solitamente 8x8 pixel)
        _spriteRegistry.Add("tile_wall",    new SpriteMetadata(SourceY: 112, Width: 8, Height: 8));
        _spriteRegistry.Add("tile_dot",     new SpriteMetadata(SourceY: 120, Width: 8, Height: 8));
        _spriteRegistry.Add("tile_gate",    new SpriteMetadata(SourceY: 120, Width: 8, Height: 8));
    }

    public void BeginDrawing()
    {
        Raylib.BeginDrawing();
        Raylib.ClearBackground(Color.Black);
    }
    public void EndDrawing() => Raylib.EndDrawing();

    public void DrawTile(int tileType, int tileVariant, int pixelX, int pixelY, int scale)
    {
        string? key = tileType switch
        {
            0 or 2 or 3 => "tile_dot",
            1 => "tile_wall",
            4 => "tile_gate",
            _ => null
        };

        if (key == null || !_spriteRegistry.TryGetValue(key, out var meta)) return;

        float sourceX = tileType switch
        {
            0 => 16, // Cella vuota
            1 => WallAtlasFrameByMask[tileVariant & 0x0F] * meta.Width,
            2 => 0,  // Pallino normale
            3 => 8,  // Power pellet
            4 => tileVariant == 1 ? 32 : 24, // Metà destra/sinistra della porta
            _ => 0
        };

        Rectangle sourceRec = new(sourceX, meta.SourceY, meta.Width, meta.Height);
        Rectangle destRec = new(pixelX, pixelY, meta.Width * scale, meta.Height * scale);

        Raylib.DrawTexturePro(_mainSpritesheet, sourceRec, destRec, Vector2.Zero, 0f, Color.White);
    }

    public void DrawSprite(string key, int pixelX, int pixelY, float rotationAngle, int frameIndex, int scale)
    {
        // Se la chiave richiede varianti direzionali per gli occhi (es: ghost_eyes_up), 
        // possiamo isolare la radice "ghost_eyes" e usare il frameIndex o la stringa per calcolare l'offset.
        string baseKey = key.StartsWith("ghost_eyes") ? "ghost_eyes" : key;

        if (!_spriteRegistry.TryGetValue(baseKey, out var meta)) return;

        // Calcola la coordinata X in base al frame di animazione corrente (es: 16px * frameIndex)
        float sourceX = frameIndex * meta.Width;

        // Se stiamo gestendo gli occhi dei fantasmi, usiamo il frameIndex per spostarci sulla riga orizzontale
        // dove sono disegnati gli occhi nelle 4 direzioni (Guarda a Sx, Su, Dx, Giù)
        if (baseKey == "ghost_eyes")
        {
            int directionOffset = key switch
            {
                "ghost_eyes_left"  => 0,
                "ghost_eyes_up"    => 1,
                "ghost_eyes_right" => 2,
                "ghost_eyes_down"  => 3,
                _                  => 0
            };
            sourceX = directionOffset * meta.Width;
            frameIndex = 0; // Azzera il frame dei piedini per gli occhi staccati
        }

        Rectangle sourceRec = new(sourceX, meta.SourceY, meta.Width, meta.Height);
        
        // Impostiamo il rettangolo di destinazione sullo schermo. 
        // Aggiungiamo metà dimensione dello sprite alle coordinate X e Y di destinazione 
        // perché vogliamo che il perno di rotazione (origin) sia esattamente al centro del personaggio!
        float halfWidthScaled = (meta.Width / 2f) * scale;
        float halfHeightScaled = (meta.Height / 2f) * scale;
        
        Rectangle destRec = new(
            pixelX + halfWidthScaled, 
            pixelY + halfHeightScaled, 
            meta.Width * scale, 
            meta.Height * scale
        );

        // L'origine locale rispetto al rettangolo destRec: posizionandolo a metà, 
        // Raylib effettuerà la rotazione geometrica attorno al baricentro dello sprite.
        Vector2 origin = new(halfWidthScaled, halfHeightScaled);

        Raylib.DrawTexturePro(_mainSpritesheet, sourceRec, destRec, origin, rotationAngle, Color.White);
    }

    public void DrawSpriteRotated(string spriteKey, int pixelX, int pixelY, float rotationAngle, int frameIndex, int scale)
    {
        DrawSprite(spriteKey, pixelX, pixelY, rotationAngle, frameIndex, scale);
    }

    public void DrawText(string text, int pixelX, int pixelY, int height, Color color)
    {
        Raylib.DrawText(text, pixelX, pixelY, height, color);
    }

    public void DrawCenterText(string text, int pixelX, int pixelY, int height, Color color)
    {
        var textLenght = Raylib.MeasureText(text, height);
        Raylib.DrawText(text, pixelX - textLenght / 2, pixelY, height, color);
    }

    public void DrawRightText(string text, int pixelX, int pixelY, int height, Color color)
    {
        var textLenght = Raylib.MeasureText(text, height);
        Raylib.DrawText(text, pixelX - textLenght, pixelY, height, color);
    }

    public void Close()
    {
        // Release the video memory before closing the application
        Raylib.UnloadTexture(_mainSpritesheet);
    }

    // Internal record to keep the pixels coordinates
    private record SpriteMetadata(float SourceY, float Width, float Height);

} 
