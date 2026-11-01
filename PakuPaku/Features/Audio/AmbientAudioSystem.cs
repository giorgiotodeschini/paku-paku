using PakuPaku.Common.Audio;
using PakuPaku.Domain;

namespace PakuPaku.Features.Audio;

public sealed class AmbientAudioSystem(
    GameState gameState,
    IAudioService audioService)
    : IAmbientAudioSystem
{
    private readonly GameState _gameState = gameState;
    private readonly IAudioService _audioService = audioService;

    public void Execute()
    {
        if (_gameState.IsGameOver || _gameState.ShouldResetRound) return;

        // Se c'è almeno un fantasma spaventato, suona la sirena del panico
        bool anyGhostScared = _gameState.Ghosts.Any(g => g.CurrentMode == GhostMode.Frightened);

        if (anyGhostScared)
        {
            _audioService.StopLoopingSound("siren_normal");
            _audioService.PlayLoopingSound("siren_scared");
        }
        else
        {
            _audioService.StopLoopingSound("siren_scared");
            _audioService.PlayLoopingSound("siren_normal");
        }
    }
}