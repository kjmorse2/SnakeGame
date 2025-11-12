using System.Text.Json.Serialization;

namespace CS3500.Snake.Models;

public class Snake
{
    /// <summary>
    /// Gets or sets a unique identifier for the snake.
    /// </summary>
    [JsonPropertyName("snake")]
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the name of the player controlling this snake.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } 

    /// <summary>
    /// Gets or sets the list of points representing the snake's body.
    /// </summary>
    [JsonPropertyName("body")]
    public List<Point2D> Body { get; set; } = new();

    /// <summary>
    /// Gets or sets the direction vector of the snake.
    /// </summary>
    [JsonPropertyName("dir")]
    public Point2D Dir { get; set; } = new();

    /// <summary>
    /// Gets or sets the score of the snake.
    /// </summary>
    [JsonPropertyName("score")]
    public int Score { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the snake has died.
    /// This property is true only on the exact tick the snake dies.
    /// </summary>
    [JsonPropertyName("died")]
    public bool Died { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the snake is alive.
    /// </summary>
    [JsonPropertyName("alive")]
    public bool Alive { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the snake is disconnected.
    /// Used to remove players from the model when they leave the game.
    /// </summary>
    [JsonPropertyName("dc")]
    public bool Dc { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the snake has joined the game.
    /// This property is true only on the exact tick the snake joins.
    /// </summary>
    [JsonPropertyName("join")]
    public bool Join { get; set; }
}
