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
    public World(int size )//, NetworkConnection connection)
    {
        this.Size = size;
        //while (true)
        //{
        //    try
        //    {
        //        UpdateElement(connection.ReceiveLine());
        //    }
        //    catch (JsonException)
        //    {
        //        break;
        //    }
        //    catch (InvalidOperationException)
        //    {
        //        // TODO handle disconnect gracefully
        //        break;
        //    }
        //    catch (NullReferenceException)
        //    {
        //        // TODO if you got here something else happened.
        //        break;
        //    }
        //}
    }

    /// <summary>
    /// Updates the corresponding game element based on the provided JSON string.
    /// </summary>
    /// <remarks>The method deserializes the JSON string into the appropriate object type based on the type
    /// identifier ('s', 'p', or 'w') and updates the corresponding collection with the deserialized object. The object
    /// is identified by its unique ID.</remarks>
    /// <param name="jsonString">A JSON-formatted string representing the game element to update. The third character in the string determines
    /// the type of the element: 's' for a snake, 'p' for a power-up, and 'w' for a wall.</param>
    public void UpdateElement(string jsonString)
    {
        char type = jsonString[2];
        switch(type)
        {
            case 's':
               Snake receivedSnake = JsonSerializer.Deserialize<Snake>(jsonString);
                Snakes[receivedSnake.Id] = receivedSnake;
                break;
            case 'p':
                PowerUp receivedPowerUp = JsonSerializer.Deserialize<PowerUp>(jsonString);
                PowerUps[receivedPowerUp.Id] = receivedPowerUp;
                break;
            case'w':
                Wall receivedWall = JsonSerializer.Deserialize<Wall>(jsonString);
                Walls[receivedWall.Id] = receivedWall;
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
