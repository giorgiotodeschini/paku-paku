namespace PakuPaku.Common.Window;

public interface IGameWindow
{
    void Initialize(int width, int height, string title);
    bool ShouldClose();
    void Close();
}
