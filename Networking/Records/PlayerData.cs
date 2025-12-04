// <copyright file="PlayerData.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System.Globalization;

namespace CS3500.Networking.Records;

public sealed class PlayerData
{
    public PlayerData(int playerId, int gameId, string name, int highScore, DateTime startTime, DateTime endTime)
    {
        PlayerId = playerId;
        GameId = gameId;
        Name = name;
        HighScore = highScore;
        StartTime = startTime;
        EndTime = endTime;
    }

    public DateTime EndTime { get; }

    public int GameId { get; }

    public int HighScore { get; }

    public string Name { get; }

    public int PlayerId { get; }

    public DateTime StartTime { get; }

    public List<string> ToStringList()
    {
        return new List<string>
        {
            PlayerId.ToString(),
            Name,
            HighScore.ToString(),
            StartTime.ToString(CultureInfo.InvariantCulture),
            EndTime.ToString(CultureInfo.InvariantCulture),
        };
    }
}
