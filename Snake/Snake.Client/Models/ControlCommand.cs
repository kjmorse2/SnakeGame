// <copyright file="ControlCommand.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System.Text.Json.Serialization;

namespace CS3500.Snake.Models;

/// <summary>
/// TODO Document ControlCommand class
/// </summary>
public class ControlCommand
{
    public ControlCommand()
    {
        Direction = string.Empty;
    }

    public ControlCommand(string direction)
    {
        this.Direction =  direction;
    }
    /// <summary>
    /// <p>
    /// Gets or sets the direction the snake is moving.
    /// </p>
    /// Possible values are:
    /// <list type="bullet">
    /// "up" - Move the snake up.
    /// "down" - Move the snake down.
    /// "left" - Move the snake left.
    /// "right" - Move the snake right.
    /// </list>
    /// </summary>
    [JsonPropertyName("moving")]
    public string Direction { get; set; }
}
