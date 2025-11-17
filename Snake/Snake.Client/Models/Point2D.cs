using System.Text.Json.Serialization;

namespace CS3500.Snake.Models;

/// <summary>
/// Represents an integer coordinate in the game world. Immutable after construction for thread safety.
/// </summary>
public class Point2D
{
    /// <summary>
    /// Gets the X coordinate component.
    /// </summary>
    [JsonPropertyName("X")]
    public int X { get; init; }

    /// <summary>
    /// Gets the Y coordinate component.
    /// </summary>
    [JsonPropertyName("Y")]
    public int Y { get; init; }
}
