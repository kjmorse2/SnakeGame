// <copyright file="Point2D.cs" company="U of U CS3500">
// Copyright (c) U of U CS3500, Kenneth Morse, and Hunter Simmons. All rights reserved.
// </copyright>

using System.Text.Json.Serialization;

namespace CS3500.SnakeModels;

/// <summary>
///     Represents an integer coordinate in the game world. Immutable after construction for thread safety.
/// </summary>
public class Point2D
{
    /// <summary>
    ///     Gets the X coordinate component.
    /// </summary>
    [JsonPropertyName("X")]
    public int X { get; init; }

    /// <summary>
    ///     Gets the Y coordinate component.
    /// </summary>
    [JsonPropertyName("Y")]
    public int Y { get; init; }
}
