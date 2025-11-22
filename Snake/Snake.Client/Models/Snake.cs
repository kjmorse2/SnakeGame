#pragma warning disable SA1636

// <copyright file="PowerUp.cs" company="U of U CS3500">
#pragma warning restore SA1636

// Copyright (c) U of U CS3500, Kenneth Morse, and Hunter Simmons. All rights reserved.
// </copyright>
using System.Text.Json.Serialization;

namespace CS3500.Snake.Models;

/// <summary>
///     Represents a player-controlled snake composed of an ordered list of body segment coordinates
///     (tail first, head last). Transient flags (Died, Join, Dc) indicate state transitions for a single tick.
/// </summary>
public class Snake
{
    /// <summary>
    ///     Gets the head coordinate (last element of <see cref="Body" />).
    /// </summary>
    public Point2D Head => Body[ ^1 ];

    /// <summary>
    ///     Gets the tail coordinate (first element of <see cref="Body" />).
    /// </summary>
    public Point2D Tail => Body[ 0 ];

    /// <summary>
    ///     Gets a value indicating whether the snake is alive (inverse of permanent death condition).
    /// </summary>
    [JsonPropertyName("alive")]
    public bool Alive { get; init; }

    /// <summary>
    ///     Gets the ordered body segment coordinates; index 0 is the tail, last index is the head.
    /// </summary>
    [JsonPropertyName("body")]
    public List<Point2D> Body { get; init; } = new();

    /// <summary>
    ///     Gets a value indicating whether the player disconnected (true only on the disconnect tick).
    /// </summary>
    [JsonPropertyName("dc")]
    public bool Dc { get; init; }

    /// <summary>
    ///     Gets a value indicating whether the snake has died (true only on the death tick).
    /// </summary>
    [JsonPropertyName("died")]
    public bool Died { get; init; }

    /// <summary>
    ///     Gets the current movement direction vector (delta per tick) reported by the server.
    /// </summary>
    [JsonPropertyName("dir")]
    public Point2D Dir { get; init; } = new();

    /// <summary>
    ///     Gets the unique identifier for the snake.
    /// </summary>
    [JsonPropertyName("snake")]
    public int Id { get; init; }

    /// <summary>
    ///     Gets a value indicating whether the player just joined (true only on the join tick).
    /// </summary>
    [JsonPropertyName("join")]
    public bool Join { get; init; }

    /// <summary>
    ///     Gets the display name of the player controlling this snake.
    /// </summary>
    [JsonPropertyName("name")]
    public required string Name { get; init; }

    /// <summary>
    ///     Gets the accumulated score for this snake.
    /// </summary>
    [JsonPropertyName("score")]
    public int Score { get; init; }
}
