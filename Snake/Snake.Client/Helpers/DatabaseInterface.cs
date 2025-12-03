// <copyright file="DatabaseInterface.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System.Data;

namespace CS3500.Snake.Client.Pages.SnakeGame;

using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using CS3500.Snake.Models;

/// <summary>
/// A database interface for the Snake game.
/// </summary>
public class DatabaseInterface
{
    private readonly SqlConnection connection;
    private DateTime startTime;
    private int currentGameId = -1;
    private static readonly string At = "@";

    /// <summary>
    /// Initializes a new instance of the <see cref="DatabaseInterface"/> class.
    /// </summary>
    public DatabaseInterface()
    {
        var builder = new ConfigurationBuilder();

        // Bind user secrets to this assembly/type to resolve the generic type symbol
        builder.AddUserSecrets<DatabaseInterface>();
        IConfigurationRoot configuration = builder.Build();
        var selectedSecrets = configuration.GetSection("SnakeSecrets");

        string connectionString = new SqlConnectionStringBuilder
        {
            DataSource = "cs3500.eng.utah.edu, 14330",
            InitialCatalog = selectedSecrets["DB_Name"],
            UserID = selectedSecrets["Username"],
            Password = selectedSecrets["Password"],
            ConnectTimeout = 15,
            Encrypt = false,
        }.ConnectionString;

        this.connection = new SqlConnection(connectionString);
        this.connection.Open();
    }

    /// <summary>
    /// Marks the start of a game session.
    /// </summary>
    public int NewGame()
    {
        this.startTime = DateTime.Now;

        EnsureOpenConnection();

        // Insert with OUTPUT to get the identity in one round trip
        string insertSql = $"INSERT INTO dbo.GameTable (StartTime) OUTPUT INSERTED.GameId VALUES ({At}StartTime)";
        using var command = new SqlCommand(insertSql, this.connection);
        command.Parameters.Add("@StartTime", SqlDbType.DateTime2).Value = this.startTime;

        object? scalar = command.ExecuteScalar();
        this.currentGameId = Convert.ToInt32(scalar);
        return this.currentGameId;
    }

    /// <summary>
    /// Records the end of a game session by updating EndTime for the current game.
    /// </summary>
    public void EndGame()
    {
        DateTime endTime = DateTime.Now;

        EnsureOpenConnection();

        string updateSql = $"UPDATE dbo.GameTable SET EndTime = {At}EndTime WHERE GameId = @GameId";
        using var command = new SqlCommand(updateSql, this.connection);
        _ = command.Parameters.Add("@EndTime", SqlDbType.DateTime).Value = endTime;
        _ = command.Parameters.Add("@GameId", SqlDbType.Int).Value = this.currentGameId;
        _ = command.ExecuteNonQueryAsync();
    }

    private void EnsureOpenConnection()
    {
        if (this.connection.State != ConnectionState.Open)
        {
            this.connection.Open();
        }
    }

    public void InsertNewPlayer(Snake snake)
    {
        EnsureOpenConnection();

        string insertSql = $"INSERT INTO dbo.Players (GameId, PlayerId, Name, MaxScore, EnterTime) VALUES ({At}GameId, {At}PlayerId, {At}Name, {At}MaxScore, {At}EnterTime)";
        using var command = new SqlCommand(insertSql, this.connection);
        command.Parameters.Add("@GameId", SqlDbType.Int).Value = this.currentGameId;
        command.Parameters.Add("@PlayerId", SqlDbType.Int).Value = snake.Id;
        command.Parameters.Add("@Name", SqlDbType.VarChar).Value = snake.Name;
        command.Parameters.Add("@MaxScore", SqlDbType.Int).Value = snake.Score;
        command.Parameters.Add("@EnterTime", SqlDbType.DateTime).Value = DateTime.Now;

        _ = command.ExecuteNonQueryAsync();
    }
    
    public void UpdatePlayerScore(int playerId, int newScore)
    {
        EnsureOpenConnection();

        string updateSql = $"UPDATE dbo.Players SET MaxScore = {At}MaxScore WHERE GameId = {At}GameId AND PlayerId = {At}PlayerId";
        using var command = new SqlCommand(updateSql, this.connection);
        command.Parameters.Add("@MaxScore", SqlDbType.Int).Value = newScore;
        command.Parameters.Add("@GameId", SqlDbType.Int).Value = this.currentGameId;
        command.Parameters.Add("@PlayerId", SqlDbType.Int).Value = playerId;

        _ = command.ExecuteNonQueryAsync();
    }
    
    public void PlayerLeft(int playerId)
    {
        EnsureOpenConnection();

        string updateSql = $"UPDATE dbo.Players SET LeaveTime = {At}LeaveTime WHERE GameId = {At}GameId AND PlayerId = {At}PlayerId";
        using var command = new SqlCommand(updateSql, this.connection);
        command.Parameters.Add("@LeaveTime", SqlDbType.DateTime2).Value = DateTime.Now;
        command.Parameters.Add("@GameId", SqlDbType.Int).Value = this.currentGameId;
        command.Parameters.Add("@PlayerId", SqlDbType.Int).Value = playerId;

        _ = command.ExecuteNonQueryAsync();
    }
}
