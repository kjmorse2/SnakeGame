namespace CS3500.Snake.Models;

/// <summary>
/// TODO Document PowerUp class
/// </summary>
public class PowerUp
{
    /// <summary>
    /// Gets or sets a unique identifier for the power-up.
    /// </summary>
    [JsonPropertyName( "power" )]
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the position of the power-up.
    /// </summary>
    [JsonPropertyName( "loc" )]
    public Point2D Position { get; set;  }

    /// <summary>
    /// Gets or sets a value indicating whether the power-up is dead.
    /// This property is true only on the exact tick the power-up is collected,
    /// and should be removed from the game immediately after.
    /// </summary>
    [JsonPropertyName( "died" )]
    public bool IsDead { get; set; }
}
