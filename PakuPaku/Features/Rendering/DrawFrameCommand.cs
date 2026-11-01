using PakuPaku.Common;
using PakuPaku.Common.Audio;
using PakuPaku.Common.Graphics;
using PakuPaku.Domain;
using Raylib_cs;

namespace PakuPaku.Features.Rendering;

public sealed class DrawFrameCommand(
    GameState gameState,
    IRenderingContext context,
    IAudioService audioService) 
    : IDrawFrameCommand
{
    private readonly GameState _gameState = gameState;
    private readonly IRenderingContext _renderContext = context;
    private readonly IAudioService _audioService = audioService;
    private int _animationFrameCounter = 0;
    private const int FontSize = 10;

    public void Execute()
    {
        DrawScore();
        DrawMaze();
        DrawLives();
        DrawLevel();
        DrawPlayer();
        DrawGhosts();

        if (_gameState.ReadyTimerFrames > 0)
        {
            int pixelX = 14 * Consts.BaseSize * Consts.Scale;
            int pixelY = 20 * Consts.BaseSize * Consts.Scale;
            _renderContext.DrawCenterText("READY!", pixelX, pixelY, FontSize * Consts.Scale, Color.Yellow);
            _audioService.PlaySound("start");
            _gameState.ReadyTimerFrames--;
        }

        if (_gameState.IsGameOver)
        {
            int pixelX = 14 * Consts.BaseSize * Consts.Scale;
            int pixelY = 20 * Consts.BaseSize * Consts.Scale;
            _renderContext.DrawCenterText("GAME OVER!", pixelX, pixelY, FontSize * Consts.Scale, Color.Yellow);
        }
    }

    private void DrawScore()
    {
        var centerX = 14 * Consts.BaseSize * Consts.Scale;
        var pixelY = 2 * Consts.BaseSize * Consts.Scale;
        _renderContext.DrawCenterText("HIGH SCORE", centerX, 4, FontSize * Consts.Scale, Color.Yellow);
        _renderContext.DrawCenterText("0", centerX, pixelY, FontSize * Consts.Scale, Color.Yellow);
        
        _renderContext.DrawText(" 1 UP", 0, 4, FontSize * Consts.Scale, Color.Yellow);
        _renderContext.DrawText(_gameState.Score.ToString().PadLeft(6), 
            0, pixelY, FontSize * Consts.Scale, Color.Yellow);
        
    }

    private void DrawMaze()
    {
        int rows = _gameState.Maze.GetLength(0);
        int cols = _gameState.Maze.GetLength(1);

        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < cols; x++)
            {
                int tileType = _gameState.Maze[y, x];
                int pixelX = x * Consts.BaseSize * Consts.Scale;
                int pixelY = y * Consts.BaseSize * Consts.Scale;
                
                int tileVariant = tileType switch
                {
                    1 => GetWallMask(x, y, rows, cols),
                    4 => x > 0 && _gameState.Maze[y, x - 1] == 4 ? 1 : 0,
                    _ => 0
                };
                _renderContext.DrawTile(tileType, tileVariant, pixelX, pixelY, Consts.Scale);
            }
        }
    }

    private void DrawLives()
    {
        var rows = _gameState.Maze.GetLength(0);
        for (int i = 0; i < _gameState.Lives; i++)
        {
            int pixelX = i * Consts.BaseSize * 2 * Consts.Scale;
            int pixelY = (rows - 2) * Consts.BaseSize * Consts.Scale;
            _renderContext.DrawSprite("pacman", pixelX, pixelY, 0.0f, 1, Consts.Scale);
        }
    }

    private void DrawLevel()
    {
        var rows = _gameState.Maze.GetLength(0);
        var cols = _gameState.Maze.GetLength(1);
        int pixelX = (cols - 1) * Consts.BaseSize * Consts.Scale;
        int pixelY = (rows - 2) * Consts.BaseSize * Consts.Scale;
        _renderContext.DrawRightText($"Level {_gameState.CurrentLevel}", pixelX, pixelY, FontSize * Consts.Scale, Color.Yellow);
    }

    private int GetWallMask(int x, int y, int rows, int cols)
    {
        const int north = 1;
        const int east = 2;
        const int south = 4;
        const int west = 8;

        int mask = 0;
        if (y > 0 && _gameState.Maze[y - 1, x] == 1) mask |= north;
        if (x < cols - 1 && _gameState.Maze[y, x + 1] == 1) mask |= east;
        if (y < rows - 1 && _gameState.Maze[y + 1, x] == 1) mask |= south;
        if (x > 0 && _gameState.Maze[y, x - 1] == 1) mask |= west;

        return mask;
    }

    private void DrawPlayer()
    {
        var state = _gameState.Player.State;

        // Calculation of the FLUID screen position in pixels by combining Tiles and Sub-Pixels.
        // We subtract 4 pixels (multiplied by the scale) because the Pacman sprite is 16x16., 
        // così da centrarlo perfettamente sul tile logico di 8x8.
        int pixelX = ((state.TileX * Consts.BaseSize + state.SubPixelX) - 4) * Consts.Scale;
        int pixelY = ((state.TileY * Consts.BaseSize + state.SubPixelY) - 4) * Consts.Scale;

        // Rotation calculation based on current direction
        // Note: Raylib rotates clockwise. Assume the original sprite is drawn facing LEFT (historical default).
        float rotation = state.CurrentDirection switch
        {
            Direction.Left  => 0f,
            Direction.Up    => 90f,
            Direction.Right => 180f,
            Direction.Down  => 270f,
            _               => 0f // If None, keep the last rotation or look to the left
        };

        // Mouth animation (only when Pac-Man is moving)
        if (state.CurrentDirection != Direction.None && _gameState.FreezeFrames == 0)
        {
            _animationFrameCounter++;
        }

        // Map the frames: 0 (cclosed), 1 (semi-open), 2 (open), 3 (semi-open)
        // We change frames every 4 chart ticks.
        int animationSequence = (_animationFrameCounter / 4) % 4; 
        int frameIndex = animationSequence switch
        {
            0 => 0, // Mouth closed
            1 => 1, // Semi-open
            2 => 2, // Open
            3 => 1, // Return semi-open
            _ => 0
        };
        
        _renderContext.DrawSpriteRotated("pacman", pixelX, pixelY, rotation, frameIndex, Consts.Scale);
    }

    private void DrawGhosts()
    {
        foreach (var ghost in _gameState.Ghosts)
        {
            // Fluid ghost movement (also centered, like Pac-Man)
            int pixelX = ((ghost.State.TileX * Consts.BaseSize + ghost.State.SubPixelX) - 4) * Consts.Scale;
            int pixelY = ((ghost.State.TileY * Consts.BaseSize + ghost.State.SubPixelY) - 4) * Consts.Scale;

            // Ghosts animation: they continuously move their "little feet" (2 frames)
            int ghostFrame = (Raylib_cs.Raylib.GetTime() * 8) % 2 == 0 ? 0 : 1;

            // Determine the sprite key based on the state (e.g., if scared, it turns blue and is the same for everyone).
            string spriteKey = ghost.CurrentMode switch
            {
                GhostMode.Frightened => "ghost_scared",
                GhostMode.Eaten => ghost.State.CurrentDirection switch
                {
                    Direction.Up  => "ghost_eyes_up",
                    Direction.Right => "ghost_eyes_right",
                    Direction.Down => "ghost_eyes_down",
                    _  => "ghost_eyes_left"
                },
                _ => $"ghost_{ghost.Type.ToString().ToLower()}" // ghost_blinky, ghost_pinky, ecc.
            };

            // Ghosts don't rotate their entire bodies; they only change the direction of their eyes! 
            // You can pass the current direction so that the renderer selects the correct eye texture.
            _renderContext.DrawSpriteRotated(spriteKey, pixelX, pixelY, 0f, ghostFrame, Consts.Scale);
        }
    }
}
