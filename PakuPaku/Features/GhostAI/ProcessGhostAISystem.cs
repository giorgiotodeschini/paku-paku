using PakuPaku.Domain;

namespace PakuPaku.Features.GhostAI;

public sealed class ProcessGhostAISystem(GameState gameState) : IProcessGhostAISystem
{
    private readonly GameState _gameState = gameState;
    public void Execute()
    {
        foreach (var ghost in _gameState.Ghosts)
        {
            // The IA choose the direction only when the ghost is exactly ton the center of the tile
            if (ghost.State.SubPixelX == 0 && ghost.State.SubPixelY == 0)
            {
                // 1Update the Target Tile based on the ghost mode and the ghost type
                UpdateGhostTarget(ghost);

                // Choose the next direction based on the targhe and the navigation rules
                ghost.State.NextDirection = CalculateNextDirection(ghost);
            }
        }
    }

    private void UpdateGhostTarget(Ghost ghost)
    {
        // If the ghost has been eaten and is in the "Eyes Only" state
        if (ghost.CurrentMode == GhostMode.Eaten)
        {
            // If it reaches the cage door, it regenerates and exits in normal mode.
            if (ghost.State.TileX == 14 && ghost.State.TileY == 14 && ghost.State.SubPixelX == 0 && ghost.State.SubPixelY == 0)
            {
                ghost.CurrentMode = GhostMode.Chase; 
                ghost.State.Speed = 1;               // Restopre notmal speed
                return;
            }

            // Its fixed target is the door of the central Ghost House.
            ghost.TargetTileX = 14;
            ghost.TargetTileY = 14;
            return;
        }

        // If the game is in SCATTER mode, each ghost points rigidly toward its own fixed corner outside the map.
        if (ghost.CurrentMode == GhostMode.Scatter)
        {
            (ghost.TargetTileX, ghost.TargetTileY) = ghost.Type switch
            {
                GhostType.Blinky => (25, -3),  // Top-right corner
                GhostType.Pinky  => (2, -3),   // Top-left corner
                GhostType.Inky   => (27, 31),  // Bottom-right corner
                GhostType.Clyde  => (0, 31),   // Bottom-left corner
                _ => (0, 0)
            };
            return;
        }

        // Se in modalità CHASE (Inseguimento), applichiamo le personalità storiche
        if (ghost.CurrentMode == GhostMode.Chase)
        {
            var pX = _gameState.Player.State.TileX;
            var pY = _gameState.Player.State.TileY;
            var pDir = _gameState.Player.State.CurrentDirection;

            switch (ghost.Type)
            {
                case GhostType.Blinky:
                    ghost.TargetTileX = pX;
                    ghost.TargetTileY = pY;
                    break;

                case GhostType.Pinky:
                    var (pAheadX, pAheadY) = GetTileAhead(pX, pY, pDir, 4);
                    ghost.TargetTileX = pAheadX;
                    ghost.TargetTileY = pAheadY;
                    break;

                case GhostType.Inky:
                    // 2 tile next to pacman
                    var (offsetX, offsetY) = GetTileAhead(pX, pY, pDir, 2);
                    // Vector from Blinky to that point and double it
                    var blinky = _gameState.Ghosts.First(g => g.Type == GhostType.Blinky);
                    ghost.TargetTileX = blinky.State.TileX + (offsetX - blinky.State.TileX) * 2;
                    ghost.TargetTileY = blinky.State.TileY + (offsetY - blinky.State.TileY) * 2;
                    break;

                case GhostType.Clyde:
                    double distance = Math.Sqrt(Math.Pow(ghost.State.TileX - pX, 2) + Math.Pow(ghost.State.TileY - pY, 2));
                    if (distance > 8)
                    {
                        ghost.TargetTileX = pX;
                        ghost.TargetTileY = pY;
                    }
                    else
                    {
                        ghost.TargetTileX = 0;  // He breaks away towards his bottom-left corner.
                        ghost.TargetTileY = 31;
                    }
                    break;
            }
        }
    }

    private static (int x, int y) GetTileAhead(int x, int y, Direction dir, int tiles)
    {
        return dir switch
        {
            Direction.Up => (x - tiles, y - tiles),  // The original bug of Pinky/Inky: If they move upwards, it also shifts to the left by the same factor.
            Direction.Down => (x, y + tiles),
            Direction.Left => (x - tiles, y),
            Direction.Right => (x + tiles, y),
            _ => (x, y),
        };
    }

    private Direction CalculateNextDirection(Ghost ghost)
    {
        // List the possible directions, excluding the one opposite to the current direction of travel (reverse direction is prohibited).
        var directionsToCheck = new List<Direction> { Direction.Up, Direction.Left, Direction.Down, Direction.Right };
        Direction opposite = GetOpposite(ghost.State.CurrentDirection);
        directionsToCheck.Remove(opposite);

        Direction bestDirection = ghost.State.CurrentDirection;
        double minDistance = double.MaxValue;

        foreach (var dir in directionsToCheck)
        {
            int nextTileX = ghost.State.TileX;
            int nextTileY = ghost.State.TileY;

            switch (dir)
            {
                case Direction.Up:    nextTileY--; break;
                case Direction.Down:  nextTileY++; break;
                case Direction.Left:  nextTileX--; break;
                case Direction.Right: nextTileX++; break;
            }

            // Validate
            if (nextTileX >= 0 && nextTileX < 28 && nextTileY >= 0 && nextTileY < 36)
            {
                if (_gameState.Maze[nextTileY, nextTileX] != 1)
                {
                    // Calculate Pythagorean distance from the target tile.
                    double dist = Math.Pow(nextTileX - ghost.TargetTileX, 2) + Math.Pow(nextTileY - ghost.TargetTileY, 2);
                    
                    // Note: In the event of equal distances, the original game follows a fixed turning priority: UP, LEFT, DOWN, RIGHT.
                    if (dist < minDistance)
                    {
                        minDistance = dist;
                        bestDirection = dir;
                    }
                }
            }
        }

        return bestDirection;
    }

    private Direction GetOpposite(Direction d) => d switch
    {
        Direction.Up => Direction.Down,
        Direction.Down => Direction.Up,
        Direction.Left => Direction.Right,
        Direction.Right => Direction.Left,
        _ => Direction.None
    };
}