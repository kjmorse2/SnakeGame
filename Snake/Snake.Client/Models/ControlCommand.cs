#pragma warning disable SA1636
// <copyright file="PowerUp.cs" company="U of U CS3500">
#pragma warning restore SA1636
// Copyright (c) U of U CS3500, Kenneth Morse, and Hunter Simmons. All rights reserved.
// </copyright>
using System.Text.Json.Serialization;

namespace CS3500.Snake.Models;

/// <summary>
///     Represents a client control command indicating the desired movement direction for a snake.
///     Serialized and sent to the server; the server interprets the direction string.
/// </summary>
public class ControlCommand(string direction)
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ControlCommand" /> class with an empty direction.
    ///     Required for JSON deserialization.
    /// </summary>
    public ControlCommand()
        : this(string.Empty)
    {
    }

    /// <summary>
    ///     Gets or sets the movement direction requested by the player.
    ///     Valid values: "up", "down", "left", "right". An empty string represents no movement.
    /// </summary>
    [JsonPropertyName("moving")]
    public string Direction { get; set; } = direction;
}
