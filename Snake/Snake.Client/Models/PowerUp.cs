#pragma warning disable SA1636

// <copyright file="PowerUp.cs" company="U of U CS3500">
#pragma warning restore SA1636

// Copyright (c) U of U CS3500, Kenneth Morse, and Hunter Simmons. All rights reserved.
// </copyright>
using System.Text.Json.Serialization;

namespace CS3500.Snake.Models;

/// <summary>
///     Represents a collectible power-up item in the world grid. When collected it is marked dead for one tick
///     so clients can remove it from their local model.
/// </summary>
public class PowerUp
{
    /// <summary>
    ///     Gets the unique identifier for the power-up.
    /// </summary>
    [JsonPropertyName("power")]
    public int Id { get; init; }

    /// <summary>
    ///     Gets a value indicating whether the power-up has been collected (dead). True only on the collection tick.
    /// </summary>
    [JsonPropertyName("died")]
    public bool IsDead { get; init; }

    /// <summary>
    ///     Gets the absolute world position where the power-up resides.
    /// </summary>
    [JsonPropertyName("loc")]
    public required Point2D Position { get; init; }
}
