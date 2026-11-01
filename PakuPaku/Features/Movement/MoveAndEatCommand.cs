using PakuPaku.Common;
using PakuPaku.Domain;

namespace PakuPaku.Features.Movement;

public sealed class MoveAndEatCommand(GameState gameState, MovementEngine movementEngine) 
    : IMoveAndEatCommand
{
    private readonly GameState _gameState = gameState;
    private readonly MovementEngine _movementEngine = movementEngine;

    public void Execute()
    {
        if (_gameState.PacmanFreezeFrames == 0)
        {
            _movementEngine.UpdateEntity(_gameState.Player.State);
        }
        else
        {
            _gameState.PacmanFreezeFrames--;
        }
    }
}