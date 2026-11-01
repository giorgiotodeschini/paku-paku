using PakuPaku.Domain;

namespace PakuPaku.Common;

public class MovementEngine(GameState gameState)
{
    private readonly GameState _gameState = gameState;

    public void UpdateEntity(EntityState entity)
    {
        // If the entity has no direction, do nothing
        if (entity.CurrentDirection == Direction.None && entity.NextDirection == Direction.None)
        {
            return;
        }

        // If the entity is positioned EXACTLY at the center of the logical tile (0,0),
        // it is time to apply the turn decided earlier (Input or AI)
        if (entity.SubPixelX == 0 && entity.SubPixelY == 0)
        {
            // Check if the next direction is valid (no walls) 
            if (entity.NextDirection != Direction.None && CanMoveInDirection(entity, entity.NextDirection))
            {
                entity.CurrentDirection = entity.NextDirection;
            }

            // If the current direction leads into a wall, the entity stops immediately.
            if (!CanMoveInDirection(entity, entity.CurrentDirection))
            {
                entity.CurrentDirection = Direction.None;
                return;
            }
        }

        // Apply sub-pixel displacement based on the entity's speed.
        ApplySubPixelStep(entity);
    }

    private bool CanMoveInDirection(EntityState entity, Direction dir)
    {
        int targetX = entity.TileX;
        int targetY = entity.TileY;

        switch (dir)
        {
            case Direction.Up:    targetY--; break;
            case Direction.Down:  targetY++; break;
            case Direction.Left:  targetX--; break;
            case Direction.Right: targetX++; break;
            case Direction.None:  return true;
        }

        // Safety handling for side tunnels: if it exits the horizontal boundaries, the movement is permitted.
        if (targetX < 0 || targetX >= 28) return true;
        
        // Check the vertical boundaries of the maze matrix
        if (targetY < 0 || targetY >= _gameState.Maze.GetLength(0)) return false;

        // 1 = Wall. If it is different from 1, the way is clear.
        return _gameState.Maze[targetY, targetX] != 1;
    }

    private static void ApplySubPixelStep(EntityState entity)
    {
        // Apply the displacement based on the entity's speed (e.g., 1 sub-pixel per frame).
        switch (entity.CurrentDirection)
        {
            case Direction.Up:    
                entity.SubPixelY -= entity.Speed; 
                break;
            case Direction.Down:  
                entity.SubPixelY += entity.Speed; 
                break;
            case Direction.Left:  
                entity.SubPixelX -= entity.Speed; 
                break;
            case Direction.Right: 
                entity.SubPixelX += entity.Speed; 
                break;
        }

        // Handling the logical tile change. 
        // An original tile is 8x8 pixels. When the offset reaches 8 or -8,
        // it means we have mathematically entered the perimeter of the next tile.
        if (entity.SubPixelX >= 8)  
        { 
            entity.TileX++; 
            entity.SubPixelX = 0; 
        }
        if (entity.SubPixelX <= -8)  
        { 
            entity.TileX--; 
            entity.SubPixelX = 0; 
        }
        if (entity.SubPixelY >= 8)  
        { 
            entity.TileY++; 
            entity.SubPixelY = 0; 
        }
        if (entity.SubPixelY<= -8)  
        { 
            entity.TileY--; 
            entity.SubPixelY = 0; 
        }

        if (entity.TileX < 0)  entity.TileX = 27;
        if (entity.TileX >= 28) entity.TileX = 0;
    }
}