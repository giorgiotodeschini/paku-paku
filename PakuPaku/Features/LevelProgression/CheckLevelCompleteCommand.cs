using PakuPaku.Common.Audio;
using PakuPaku.Domain;

namespace PakuPaku.Features.LevelProgression;

public sealed class CheckLevelCompleteCommand(
    GameState gameState,
    IAudioService audioService) 
    : ICheckLevelCompleteCommand
{
    private readonly GameState _gameState = gameState;
    private readonly IAudioService _audioService = audioService;

    public void Execute()
    {
        // If there are still dots left, the level continues.
        if (_gameState.TotalDotsRemaining > 0) return;

        // Stop the audio
        _audioService.StopLoopingSound("siren_normal");
        _audioService.StopLoopingSound("siren_scared");

        // Level up
        _gameState.CurrentLevel++;
        
        // Reload the cleaned map
        _gameState.LoadInitialMap();

        // Reposition the characters at the starting points (historical spawn coordinates).
        _gameState.ResetRoundPositions();

        // Applies a brief 2-second (120-frame) freeze to give the player a moment to catch their breath.
        _gameState.GlobalFreezeFrames = 120;

        // Show the ready text in the center of the maze 
        _gameState.ReadyTimerFrames = 120;
    }
}