// <copyright file="PowerUp.cs" company="U of U CS3500">
// Copyright (c) U of U CS3500, Kenneth Morse, and Hunter Simmons. All rights reserved.
// </copyright>

using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Components;
namespace CS3500.SnakeModels;

/// <summary>
///     Represents a collectible power-up item in the world grid. When collected it is marked dead for one tick
///     so clients can remove it from their local model.
/// </summary>
public class PowerUp
{
    /// <summary>
    ///     Gets or sets the reference to the power-up image.
    /// </summary>
    public static ElementReference ImageReference { get; set; }

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
