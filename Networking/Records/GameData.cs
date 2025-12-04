// <copyright file="GameData.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System.Globalization;

namespace CS3500.Networking.Records;

/// <summary>
/// Represents data associated with a specific game, including its unique identifier, start time, and end time.
/// </summary>
/// <remarks>This class is immutable. </remarks>
public sealed class GameData
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GameData"/> class.
    /// </summary>
    /// <param name="gameId"> The ID of the game.</param>
    /// <param name="startTime">The time when the game started.</param>
    /// <param name="endTime">The time at when the game ended.</param>
    public GameData(int gameId, DateTime startTime, DateTime endTime)
    {
        GameId = gameId;
        StartTime = startTime;
        EndTime = endTime;
    }

    /// <summary>
    /// Gets the time when the game ended.
    /// </summary>
    public DateTime EndTime { get; }

    /// <summary>
    /// Gets the ID of the game.
    /// </summary>
    public int GameId { get; }

    /// <summary>
    /// Gets the time when the game started.
    /// </summary>
    public DateTime StartTime { get; }

    /// <summary>
    /// Converts the <see cref="GameData"/> into a list of strings used for HTML table row generation.
    /// </summary>
    /// <returns>A list of strings representing the object's properties. The list is ordered by  GameID, StartTime, and Endtime.
    public List<string> ToStringList()
    {
        return new List<string>
        {
            GameId.ToString(),
            StartTime.ToString(CultureInfo.InvariantCulture),
            EndTime.ToString(CultureInfo.InvariantCulture),
        };
    }
}
