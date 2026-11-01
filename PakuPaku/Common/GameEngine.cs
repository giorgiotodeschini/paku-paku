using System.Runtime.CompilerServices;
using PakuPaku.Common.Audio;
using PakuPaku.Common.Graphics;
using PakuPaku.Common.Input;
using PakuPaku.Common.Window;
using PakuPaku.Domain;
using PakuPaku.Features.Audio;
using PakuPaku.Features.CollisionAndEating;
using PakuPaku.Features.GhostAI;
using PakuPaku.Features.LevelProgression;
using PakuPaku.Features.Movement;
using PakuPaku.Features.Rendering;

namespace PakuPaku.Common;

public sealed class GameEngine(
    GameState gameState,
    IGameWindow window,
    IRenderingContext context,
    IInputSystem inputSystem,
    IAudioService audioService,
    IProcessGhostAISystem processGhostAISystem,
    IMoveAndEatCommand moveAndEatCommand,
    IMoveGhostsCommand moveGhostsCommand,
    IProcessCollisionsCommand processCollisionsCommand,
    ICheckLevelCompleteCommand checkLevelCompleteCommand,
    IDrawFrameCommand drawFrameCommand,
    IAmbientAudioSystem ambientAudioSystem) 
    : IGameEngine
{
    private readonly GameState _gameState = gameState;
    private readonly IGameWindow _window = window;
    private readonly IRenderingContext _context = context;
    private readonly IInputSystem _inputSystem = inputSystem;
    private readonly IAudioService _audioService = audioService;
    private readonly IProcessGhostAISystem _processGhostAISystem = processGhostAISystem;
    private readonly IMoveAndEatCommand _moveAndEatCommand = moveAndEatCommand;
    private readonly IMoveGhostsCommand _moveGhostsCommand = moveGhostsCommand;
    private readonly IProcessCollisionsCommand _processCollisionsCommand = processCollisionsCommand;
    private readonly ICheckLevelCompleteCommand _checkLevelCompleteCommand = checkLevelCompleteCommand;
    private readonly IDrawFrameCommand _drawFrameCommand = drawFrameCommand;
    private readonly IAmbientAudioSystem _ambientAudioSystem = ambientAudioSystem;
    
    public void Run()
    {
        _window.Initialize(700, 900, "Paku Paku");
        _context.Initialize();
        _audioService.Initialize();

        while (!_window.ShouldClose())
        {
            _inputSystem.Execute();


            // Check if there is a Global Freeze (e.g. Pac-man just eaten Blinky)
            if (_gameState.GlobalFreezeFrames > 0)
            {
                _gameState.GlobalFreezeFrames--;
            }
            else
            {
                // Ghost Scare Timer management
                if (_gameState.FrightenedTimerFrames > 0)
                {
                    _gameState.FrightenedTimerFrames--;
                    if (_gameState.FrightenedTimerFrames == 0)
                    {
                        // Time is up; the ghosts return to normal mode (Chase/Scatter).
                        ResetGhostsToNormalMode();
                    }
                }

                // Execute the ghosts IA
                _processGhostAISystem.Execute();

                // Movements
                _moveAndEatCommand.Execute();
                _moveGhostsCommand.Execute();

                // Check Collisions
                _processCollisionsCommand.Execute();

                // Check level completition
                _checkLevelCompleteCommand.Execute();

                _ambientAudioSystem.Execute();
            }

            _context.BeginDrawing();
            _drawFrameCommand.Execute();
            _context.EndDrawing();

            if (_gameState.ShouldResetRound)
            {
                _gameState.ReadyTimerFrames = 120;
                Thread.Sleep(3000);
                _gameState.ResetRoundPositions();
                _gameState.ShouldResetRound = false;
            }
        }

        _audioService.Close();
        _context.Close();
        _window.Close();
    }

    private void ResetGhostsToNormalMode()
    {
        foreach (var ghost in _gameState.Ghosts)
        {
            ghost.CurrentMode = GhostMode.Chase;
        }
    }
}