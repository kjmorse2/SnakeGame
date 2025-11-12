// <copyright file="ControlCommand.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Snake.Models;

/// <summary>
/// TODO Document ControlCommand class
/// </summary>
public class ControlCommand
{
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
    public string Moving { get; set; }

    //TODO: possibly add Enum for direction
}
