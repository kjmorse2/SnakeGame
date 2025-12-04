// <copyright file="PlayerData.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System.Globalization;

namespace CS3500.Networking.Records;

/// <summary>
///     Holds the information about a player in a specific game, which includes their ID, name, highscore,
///     and the time they joined the game and left the game.
/// </summary>
/// /
/// <remarks>This class is immutable. </remarks>
public sealed class PlayerData
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="PlayerData" /> class.
    /// </summary>
    /// <param name="playerId"> The ID of the player.</param>
    /// <param name="gameId">The ID of the games this player was in.</param>
    /// <param name="name">The display name of the player.</param>
    /// <param name="highScore">The highest score the player earned in this game.</param>
    /// <param name="startTime">The time the player joined the game.</param>
    /// <param name="endTime">The time the player left the game.</param>
    public PlayerData(int playerId, int gameId, string name, int highScore, DateTime startTime, DateTime endTime)
    {
        PlayerId = playerId;
        GameId = gameId;
        Name = name;
        HighScore = highScore;
        StartTime = startTime;
        EndTime = endTime;
    }

    /// <summary>
    ///     Gets the time when the player left the game.
    /// </summary>
    public DateTime EndTime { get; }

    /// <summary>
    ///     Gets the ID of the game that this player belongs to.
    /// </summary>
    public int GameId { get; }

    /// <summary>
    ///     Gets the high score the player earned during this game.
    /// </summary>
    public int HighScore { get; }

    /// <summary>
    ///     Gets the name of the player.
    /// </summary>
    public string Name { get; }

    /// <summary>
    ///     Gets the ID of the the player.
    /// </summary>
    public int PlayerId { get; }

    /// <summary>
    ///     Gets the time when the player joined the game.
    /// </summary>
    public DateTime StartTime { get; }

    /// <summary>
    ///     Converts the <see cref="PlayerData" /> into a list of strings used for HTML table row generation.
    /// </summary>
    /// <returns>A list of strings in the order of PlayerId, Name, StartTime, EndTime, and Dates.</returns>
    public List<string> ToStringList()
    {
        string endTimeStr = EndTime == DateTime.MinValue
            ? "Not Left Yet"
            : EndTime.ToString(CultureInfo.InvariantCulture);
        return
        [
            PlayerId.ToString(),
            Name,
            HighScore.ToString(),
            StartTime.ToString(CultureInfo.InvariantCulture),
            endTimeStr,
        ];
    }
}
