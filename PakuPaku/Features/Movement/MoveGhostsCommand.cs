using PakuPaku.Common;
using PakuPaku.Domain;

namespace PakuPaku.Features.Movement;

public sealed class MoveGhostsCommand(GameState gameState, MovementEngine movementEngine) 
    : IMoveGhostsCommand
{
    private readonly GameState _gameState = gameState;
    private readonly MovementEngine _movementEngine = movementEngine;
    
    public void Execute()
    {
        foreach (var ghost in _gameState.Ghosts)
        {
            _movementEngine.UpdateEntity(ghost.State);
        }
    }
}