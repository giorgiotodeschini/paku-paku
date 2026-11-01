namespace PakuPaku.Common.Audio;

public interface IAudioService
{
    void Initialize();
    void Close();

    void PlaySound(string key);
    void PlayLoopingSound(string key);
    void StopLoopingSound(string key);
}