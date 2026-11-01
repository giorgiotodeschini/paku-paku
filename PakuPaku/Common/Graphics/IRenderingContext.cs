using Raylib_cs;

namespace PakuPaku.Common.Graphics;

public interface IRenderingContext
{
    void Initialize();
    void BeginDrawing();
    void EndDrawing();
    void DrawTile(int tileType, int tileVariant, int pixelX, int pixelY, int scale);
    void DrawSprite(string key, int pixelX, int pixelY, float rotationAngle, int frameIndex, int scale);
    void DrawSpriteRotated(string spriteKey, int pixelX, int pixelY, float rotationAngle, int frameIndex, int scale);
    void DrawText(string text, int pixelX, int pixelY, int height, Color color);
    void DrawCenterText(string text, int pixelX, int pixelY, int height, Color color);
    void DrawRightText(string text, int pixelX, int pixelY, int height, Color color);
    void Close();
}
