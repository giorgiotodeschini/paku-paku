using Raylib_cs;

namespace PakuPaku.Common.Window;

public class RaylibWindow : IGameWindow
{
    public void Initialize(int width, int height, string title)
    {
        Raylib.InitWindow(width, height, title);
        Raylib.SetTargetFPS(60);
    }

    public bool ShouldClose() => Raylib.WindowShouldClose();

    public void Close() => Raylib.CloseWindow();
}