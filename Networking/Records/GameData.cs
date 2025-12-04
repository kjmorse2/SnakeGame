// <copyright file="GameData.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System.Globalization;

namespace CS3500.Networking.Records;

public sealed class GameData
{
    public GameData(int gameId, DateTime startTime, DateTime endTime)
    {
        GameId = gameId;
        StartTime = startTime;
        EndTime = endTime;
    }

    public DateTime EndTime { get; }

    public int GameId { get; }

    public DateTime StartTime { get; }

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
