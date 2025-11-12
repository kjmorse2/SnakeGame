using System.Text.Json.Serialization;

namespace CS3500.Snake.Models;

/// <summary>
/// Notes:
///     Walls are always axis-aligned.
///     Walls can be anywhere, as long as the distance between p1 and p2 is a multiple of 50.
///     Order of p1 and p2 does not matter.
///     Walls can overlap
/// TODO Document Wall class
/// </summary>
public class Wall
{
    /// <summary>
    /// Gets or sets a unique identifier for the wall.
    /// </summary>
    [JsonPropertyName("wall")]
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the starting point of the wall.
    /// </summary>
    [JsonPropertyName("p1")]
    public Point2D Start { get; set; }

    /// <summary>
    /// Gets or sets the ending point of the wall.
    /// </summary>
    [JsonPropertyName("p2")]
    public Point2D End { get; set; }
}
