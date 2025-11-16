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
    public Point2D p1 { get; set; }

    /// <summary>
    /// Gets or sets the ending point of the wall.
    /// </summary>
    [JsonPropertyName("p2")]
    public Point2D p2 { get; set; }

    private bool IsVertical
    {
        get
        {
            return p1.X == p2.X;
        }
    }

    public Point2D Start
    {
        get
        {
            return new Point2D
            {
                X = Math.Min(p1.X, p2.X),
                Y = Math.Min(p1.Y, p2.Y)
            };
        }
    }

    public Point2D End
    {
        get
        {
            return new Point2D
            {
                X = Math.Max(p1.X, p2.X),
                Y = Math.Max(p1.Y, p2.Y)
            };
        }
    }

    public IEnumerable<Point2D> GetSegments()
    {
        if (p1 != null && p2 != null)
        {
            if (IsVertical)
            {
                for (int y = Start.Y; y <= End.Y; y += 50)
                {
                    yield return new Point2D { X = p1.X, Y = y } ;
                }
            }
            else
            {

                for (int x = Start.X; x <= End.X; x += 50)
                {
                    yield return new Point2D { X = x, Y = p1.Y };
                }
            }
        }
    }
}
