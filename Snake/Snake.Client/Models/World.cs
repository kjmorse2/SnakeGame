using System.Text.Json;

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
    public World(int size)
    {
        this.Size = size;
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
        }
    }

    /// <summary>
    /// Gets the size of this world.
    /// </summary>
    public int Size { get; }

    /// <summary>
    /// Gets a list of snakes in the current game world.
    /// </summary>
    public List<Snake> Snakes { get; } = new(100);

    /// <summary>
    /// Gets a list of walls in the current game world.
    /// </summary>
    public List<Wall> Walls { get; } = new(100);

    /// <summary>
    /// Gets a list of power-ups in the current game world.
    /// </summary>
    public List<PowerUp> PowerUps { get; } = new(100);
}
