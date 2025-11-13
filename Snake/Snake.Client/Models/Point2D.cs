using System.Text.Json.Serialization;

namespace CS3500.Snake.Models;

public class Point2D
{
    /// <summary>
    /// Gets or sets the X value of a point.
    /// </summary>
    [JsonPropertyName("X")]
    public int X { get; set; }

    /// <summary>
    /// Gets or sets the Y value of a point.
    /// </summary>
    [JsonPropertyName("Y")]
    public int Y { get; set; }
}
