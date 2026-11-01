using PakuPaku.Domain;
using Raylib_cs;

namespace PakuPaku.Common.Input;

public class RaylibInputSystem(GameState gameState) : IInputSystem
{
    private readonly GameState _gameState = gameState;
    public void Execute()
    {
        if (Raylib.IsKeyPressed(KeyboardKey.Up))    
            _gameState.Player.State.NextDirection = Direction.Up;
        if (Raylib.IsKeyPressed(KeyboardKey.Down))  
            _gameState.Player.State.NextDirection = Direction.Down;
        if (Raylib.IsKeyPressed(KeyboardKey.Left))  
            _gameState.Player.State.NextDirection = Direction.Left;
        if (Raylib.IsKeyPressed(KeyboardKey.Right)) 
            _gameState.Player.State.NextDirection = Direction.Right;

    }
}