using System.Text.Json;
using CS3500.Networking;

namespace CS3500.Snake.Models;

/// <summary>
/// TODO document.
/// </summary>
public class World
{
    /// <summary>
    /// Initializes a new instance of the <see cref="World"/> class.
    /// </summary>
    /// <param name="size">The size of the world, worlds are always square.</param>
    public World(int size, NetworkConnection connection)
    {
        this.Size = size;
        while (true)
        {
            try
            {
                UpdateElement(connection.ReceiveLine());
            }
            catch (JsonException)
            {
                break;
            }
            catch (InvalidOperationException)
            {
                // TODO handle disconnect gracefully
                break;
            }
            catch (NullReferenceException)
            {
                // TODO if you got here something else happened.
                break;
            }
        }
    }

    public void UpdateElement(string jsonString)
    {
        char type = jsonString[2];
        switch(type)
        {
            case 's':
               Snake recievedSnake = JsonSerializer.Deserialize<Snake>(jsonString);
                Snakes[recievedSnake.Id] = recievedSnake;
                break;
            case 'p':
                PowerUp recievedPowerUp = JsonSerializer.Deserialize<PowerUp>(jsonString);
                PowerUps[recievedPowerUp.Id] = recievedPowerUp;
                break;
            case'w':
                Wall recievedWall = JsonSerializer.Deserialize<Wall>(jsonString);
                Walls[recievedWall.Id] = recievedWall;
                break;
        }
    }

    /// <summary>
    /// Gets the size of this world.
    /// </summary>
    public int Size { get; }

    /// <summary>
    /// Gets a list of snakes in the current game world.
    /// </summary>
    public Dictionary<int, Snake> Snakes { get; } = new(100);

    /// <summary>
    /// Gets a list of walls in the current game world.
    /// </summary>
    public Dictionary<int, Wall> Walls { get; } = new(100);

    /// <summary>
    /// Gets a list of power-ups in the current game world.
    /// </summary>
    public Dictionary<int, PowerUp> PowerUps { get; } = new();
}
