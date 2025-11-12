using System.Text.Json.Serialization;

namespace CS3500.Snake.Models;

public class Point2D
{
    [JsonPropertyName("X")]
    public int X { get; set; }

    [JsonPropertyName("Y")]
    public int Y { get; set; }
}
