using Raylib_cs;

namespace PakuPaku.Common.Audio;

public sealed class RaylibAudioService : IAudioService
{
    private readonly Dictionary<string, Sound> _soundRegistry = [];

    public void Initialize()
    {
        Raylib.InitAudioDevice();

        string assetsPath = Path.Combine(AppContext.BaseDirectory, "Assets/Audio");
        _soundRegistry.Add("start",         Raylib.LoadSound($"{assetsPath}/start.wav"));
        _soundRegistry.Add("eat_dot",       Raylib.LoadSound($"{assetsPath}/eat_dot.wav"));
        _soundRegistry.Add("eat_ghost",     Raylib.LoadSound($"{assetsPath}/eat_ghost.wav"));
        _soundRegistry.Add("player_death",  Raylib.LoadSound($"{assetsPath}/death.wav"));
        _soundRegistry.Add("siren_normal",  Raylib.LoadSound($"{assetsPath}/siren.wav"));
        _soundRegistry.Add("siren_scared",  Raylib.LoadSound($"{assetsPath}/fright.wav"));
    }

    public void PlaySound(string key)
    {
        if (_soundRegistry.TryGetValue(key, out var sound))
        {
            if (!Raylib.IsSoundPlaying(sound))
            {
                Raylib.PlaySound(sound);
            }
        }
    }

    public void PlayLoopingSound(string key)
    {
        if (_soundRegistry.TryGetValue(key, out var sound))
        {
            if (!Raylib.IsSoundPlaying(sound))
            {
                Raylib.PlaySound(sound);
            }
        }
    }

    public void StopLoopingSound(string key)
    {
        if (_soundRegistry.TryGetValue(key, out var sound))
        {
            if (Raylib.IsSoundPlaying(sound))
            {
                Raylib.StopSound(sound);
            }
        }
    }

    public void Close()
    {
        foreach (var sound in _soundRegistry.Values)
        {
            Raylib.UnloadSound(sound);
        }
        _soundRegistry.Clear();

        Raylib.CloseAudioDevice();
    }
}