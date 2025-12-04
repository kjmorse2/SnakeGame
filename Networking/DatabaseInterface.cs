// <copyright file="DatabaseInterface.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace CS3500.Networking;

/// <summary>
///     A database interface for the Snake game.
/// </summary>
public class DatabaseInterface
{
    private static readonly string At = "@";
    private readonly SqlConnection connection;
    private int currentGameId = -1;
    private DateTime startTime;

    /// <summary>
    ///     Initializes a new instance of the <see cref="DatabaseInterface" /> class.
    /// </summary>
    public DatabaseInterface()
    {
        ConfigurationBuilder builder = new();

        // Bind user secrets to this assembly/type to resolve the generic type symbol
        builder.AddUserSecrets<DatabaseInterface>();
        IConfigurationRoot configuration = builder.Build();
        IConfigurationSection selectedSecrets = configuration.GetSection("SnakeSecrets");

        string connectionString = new SqlConnectionStringBuilder
        {
            DataSource = "cs3500.eng.utah.edu, 14330",
            InitialCatalog = selectedSecrets[ "DB_Name" ],
            UserID = selectedSecrets[ "Username" ],
            Password = selectedSecrets[ "Password" ],
            ConnectTimeout = 15,
            Encrypt = false,
        }.ConnectionString;

        connection = new SqlConnection(connectionString);
        connection.Open();
    }

    /// <summary>
    ///     Records the end of a game session by updating EndTime for the current game.
    /// </summary>
    public void EndGame()
    {
        DateTime endTime = DateTime.Now;

        EnsureOpenConnection();

        string updateGamesTable = $"UPDATE dbo.GameTable SET EndTime = {At}EndTime WHERE GameId = @GameId";
        using SqlCommand gameCommand = new(updateGamesTable, connection);
        gameCommand.Parameters.Add("@EndTime", SqlDbType.DateTime).Value = endTime;
        gameCommand.Parameters.Add("@GameId", SqlDbType.Int).Value = currentGameId;
        gameCommand.ExecuteNonQuery();

        string playersToUpdate = "SELECT PlayerId FROM dbo.Players WHERE GameId = @GameId AND LeaveTime IS NULL";
        using SqlCommand selectCommand = new(playersToUpdate, connection);
        selectCommand.Parameters.Add("@GameId", SqlDbType.Int).Value = currentGameId;
        using SqlDataReader reader = selectCommand.ExecuteReader();
        List<int> players = new();
        while (reader.Read())
        {
            players.Add(reader.GetInt32(0));
        }

        reader.Close();

        PlayersLeft(players);
    }

    /// <summary>
    ///     Inserts a new player into the database.
    /// </summary>
    /// <param name="playerId">The ID of the player.</param>
    /// <param name="name">The name of the player.</param>
    /// <param name="score">The initial score of the player.</param>
    public void InsertNewPlayer(int playerId, string name, int score)
    {
        EnsureOpenConnection();
        string insertSql =
            $"INSERT INTO dbo.Players (GameId, PlayerId, Name, MaxScore, EnterTime) VALUES ({At}GameId, {At}PlayerId, {At}Name, {At}MaxScore, {At}EnterTime)";
        using SqlCommand command = new(insertSql, connection);
        command.Parameters.Add("@GameId", SqlDbType.Int).Value = currentGameId;
        command.Parameters.Add("@PlayerId", SqlDbType.Int).Value = playerId;
        command.Parameters.Add("@Name", SqlDbType.VarChar).Value = name;
        command.Parameters.Add("@MaxScore", SqlDbType.Int).Value = score;
        command.Parameters.Add("@EnterTime", SqlDbType.DateTime).Value = DateTime.Now;
        command.ExecuteNonQuery();
    }

    /// <summary>
    ///     Marks the start of a game session.
    /// </summary>
    public int NewGame()
    {
        startTime = DateTime.Now;

        EnsureOpenConnection();

        // Insert with OUTPUT to get the identity in one round trip
        string insertSql = $"INSERT INTO dbo.GameTable (StartTime) OUTPUT INSERTED.GameId VALUES ({At}StartTime)";
        using SqlCommand command = new(insertSql, connection);
        command.Parameters.Add("@StartTime", SqlDbType.DateTime2).Value = startTime;

        object? scalar = command.ExecuteScalar();
        currentGameId = Convert.ToInt32(scalar);
        return currentGameId;
    }

    public void PlayerLeft(int playerId)
    {
        PlayerLeft(playerId, DateTime.Now);
    }

    /// <summary>
    ///     Updates the score of a player.
    /// </summary>
    /// <param name="playerId">The ID of the player.</param>
    /// <param name="newScore">The new score of the player.</param>
    /// <param name="oldScore">The old score of the player.</param>
    public void UpdatePlayerScore(int playerId, int newScore, int oldScore)
    {
        if (newScore <= oldScore)
        {
            return;
        }

        EnsureOpenConnection();
        string updateSql =
            $"UPDATE dbo.Players SET MaxScore = {At}MaxScore WHERE GameId = {At}GameId AND PlayerId = {At}PlayerId";
        using SqlCommand command = new(updateSql, connection);
        command.Parameters.Add("@MaxScore", SqlDbType.Int).Value = newScore;
        command.Parameters.Add("@GameId", SqlDbType.Int).Value = currentGameId;
        command.Parameters.Add("@PlayerId", SqlDbType.Int).Value = playerId;
        command.ExecuteNonQuery();
    }

    private void EnsureOpenConnection()
    {
        if (connection.State != ConnectionState.Open)
        {
            connection.Open();
        }
    }

    private void PlayerLeft(int playerId, DateTime endTime)
    {
        EnsureOpenConnection();

        string updateSql =
            $"UPDATE dbo.Players SET LeaveTime = {At}LeaveTime WHERE GameId = {At}GameId AND PlayerId = {At}PlayerId";
        using SqlCommand command = new(updateSql, connection);
        command.Parameters.Add("@LeaveTime", SqlDbType.DateTime2).Value = endTime;
        command.Parameters.Add("@GameId", SqlDbType.Int).Value = currentGameId;
        command.Parameters.Add("@PlayerId", SqlDbType.Int).Value = playerId;

        command.ExecuteNonQuery();
    }

    private void PlayersLeft(List<int> players)
    {
        DateTime endTime = DateTime.Now;
        foreach (int playerId in players)
        {
            PlayerLeft(playerId, endTime);
        }
    }
}
