using System.Collections.Concurrent;
using System.Text.Json;

namespace CS3500.Snake.Models;

/// <summary>
/// Represents the authoritative state of the game world for a single client: square boundary size and
/// collections of snakes, walls, and power-ups keyed by unique IDs. Collections are concurrent to allow
/// safe updates from a background receive loop while rendering.
/// </summary>
public class World
{
    /// <summary>
    /// Initializes a new instance of the <see cref="World"/> class with a square size in pixels.
    /// </summary>
    /// <param name="size">Square dimension (width == height) of the playable area.</param>
    public World(int size)
    {
        this.Size = size;
    }
    
    private static Point2D DefaultPoint => new Point2D { X = 0, Y = 0 };

    /// <summary>
    /// Deserializes and applies a JSON update for a single game element (snake, power-up, wall).
    /// The element type is inferred from the 3rd character of the JSON string (index 2).
    /// </summary>
    /// <param name="jsonString">JSON payload representing an update from the server.</param>
    /// <remarks>
    /// Expected leading type markers:
    /// 's' => <see cref="Snake"/>, 'p' => <see cref="PowerUp"/>, 'w' => <see cref="Wall"/>.
    /// TODO: Validate JSON structure and handle malformed input gracefully instead of throwing.
    /// </remarks>
    public void UpdateElement(string jsonString)
    {
        if (string.IsNullOrWhiteSpace(jsonString))
        {
            return;
        }
        char type = jsonString[2];
        try
        {
            switch (type)
            {
                case 's':
                    Snake receivedSnake = JsonSerializer.Deserialize<Snake>(jsonString) ??
                                          throw new InvalidOperationException();
                    Snakes[ receivedSnake.Id ] = receivedSnake;
                    if (receivedSnake.Dc)
                    {
                        RemoveSnakeIds.Add(receivedSnake.Id);
                    }
                    break;
                case 'p':
                    PowerUp receivedPowerUp = JsonSerializer.Deserialize<PowerUp>(jsonString) ??
                                              throw new InvalidOperationException();
                    PowerUps[ receivedPowerUp.Id ] = receivedPowerUp;
                    if (receivedPowerUp.IsDead)
                    {
                        RemovePowerUpIDs.Add(receivedPowerUp.Id);
                    }
                    break;
                case 'w':
                    Wall receivedWall = JsonSerializer.Deserialize<Wall>(jsonString) ??
                                        throw new InvalidOperationException();
                    Walls[ receivedWall.Id ] = receivedWall;
                    break;
            }
        }
        catch (JsonException e)
        {
            throw new InvalidOperationException("Failed to deserialize JSON string.", e);
        }
        catch (Exception e)
        {
            throw new InvalidOperationException("An error occurred while updating the world element.", e);
        }
    }

    public void CleanupDeadElements()
    {
        while (RemoveSnakeIds.TryTake(out int snakeId))
        {
            Snakes.TryRemove(snakeId, out _);
        }
        while (RemovePowerUpIDs.TryTake(out int powerUpId))
        {
            PowerUps.TryRemove(powerUpId, out _);
        }
    }

    public Point2D GetHead(int playerId)
    {
        if (Snakes.TryGetValue(playerId, out Snake? snake))
        {
            return snake.Head;
        }
        return DefaultPoint;
    }

    /// <summary>
    /// Gets the world square size in pixels.
    /// </summary>
    public int Size { get; }

    /// <summary>
    /// Gets the collection of snakes keyed by id.
    /// </summary>
    public ConcurrentDictionary<int, Snake> Snakes { get; } = new();

    /// <summary>
    /// Gets the collection of walls keyed by id.
    /// </summary>
    public ConcurrentDictionary<int, Wall> Walls { get; } = new();

    /// <summary>
    /// Gets the collection of power-ups keyed by id.
    /// </summary>
    public ConcurrentDictionary<int, PowerUp> PowerUps { get; } = new();

    private ConcurrentBag<int> RemoveSnakeIds { get; } = new();

    private ConcurrentBag<int> RemovePowerUpIDs { get; } = new();
}
