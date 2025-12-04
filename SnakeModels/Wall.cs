// <copyright file="Wall.cs" company="U of U CS3500">
// Copyright (c) U of U CS3500, Kenneth Morse, and Hunter Simmons. All rights reserved.
// </copyright>

using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Components;

namespace CS3500.SnakeModels;

/// <summary>
///     Represents an axis-aligned wall occupying a continuous span of 50px segments between two endpoints.
///     Endpoints may be given in any order. Overlapping walls are allowed.
/// </summary>
public class Wall
{
    /// <summary>
    ///     Size (width and height) in pixels of each wall segment.
    /// </summary>
    public static readonly int SegmentSize = 50;

    /// <summary>
    ///     Gets or sets the reference to the wall image.
    /// </summary>
    public static ElementReference ImageReference { get; set; }

    /// <summary>
    ///     Gets the unique identifier for the wall.
    /// </summary>
    [JsonPropertyName("wall")]
    public int Id { get; init; }

    /// <summary>
    ///     Gets the first endpoint (unordered) of the wall span.
    /// </summary>
    [JsonPropertyName("p1")]
    public required Point2D P1 { get; init; }

    /// <summary>
    ///     Gets the second endpoint (unordered) of the wall span.
    /// </summary>
    [JsonPropertyName("p2")]
    public required Point2D P2 { get; init; }

    /// <summary>
    ///     Gets the normalized maximum (bottom-right) endpoint regardless of input order.
    /// </summary>
    private Point2D End => new() { X = Math.Max(P1.X, P2.X), Y = Math.Max(P1.Y, P2.Y) };

    /// <summary>
    ///     Gets a value indicating whether the wall is vertical (same X coordinate).
    /// </summary>
    private bool IsVertical => P1.X == P2.X;

    /// <summary>
    ///     Gets the normalized minimum (top-left) endpoint regardless of input order.
    /// </summary>
    private Point2D Start => new() { X = Math.Min(P1.X, P2.X), Y = Math.Min(P1.Y, P2.Y) };

    /// <summary>
    ///     Enumerates each discrete 50px segment position along the wall span.
    /// </summary>
    /// <returns>Sequence of segment center points aligned to the grid.</returns>
    public IEnumerable<Point2D> GetSegments()
    {
        if (IsVertical)
        {
            for (int y = Start.Y; y <= End.Y; y += SegmentSize)
            {
                yield return new Point2D { X = P1.X, Y = y };
            }
        }
        else
        {
            for (int x = Start.X; x <= End.X; x += SegmentSize)
            {
                yield return new Point2D { X = x, Y = P1.Y };
            }
        }
    }
}
