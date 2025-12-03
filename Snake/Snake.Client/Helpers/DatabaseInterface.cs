// <copyright file="DatabaseInterface.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System.Data;

namespace CS3500.Snake.Client.Pages.SnakeGame;

using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

/// <summary>
/// A database interface for the Snake game.
/// </summary>
public class DatabaseInterface
{
    private readonly SqlConnection connection;
    private DateTime startTime;
    private int currentGameID = -1;

    /// <summary>
    /// Initializes a new instance of the <see cref="DatabaseInterface"/> class.
    /// </summary>
    public DatabaseInterface()
    {
        var builder = new ConfigurationBuilder();

        // Bind user secrets to this assembly/type to resolve the generic type symbol
        builder.AddUserSecrets<SnakeGame>();
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

        // Create instance of database connection.
        this.connection = new SqlConnection(connectionString);
        this.connection.Open();
    }

    /// <summary>
    /// Marks the start of a game session.
    /// </summary>
    public int NewGame()
    {
        int gameID = -1;
        this.startTime = DateTime.Now;
        // Build the SQL text while avoiding IDE false errors about '@' tokens in strings
        string at = "@";
        string cmdText = $"INSERT INTO dbo.GameTable (StartTime) VALUES ({at}StartTime)";
        using var command = new SqlCommand(cmdText, this.connection);
        // Bind parameters using names that match the SQL placeholders
        var pStart = command.Parameters.Add("@StartTime", System.Data.SqlDbType.DateTime);
        pStart.Value = startTime;

        _ = command.ExecuteNonQuery();
        cmdText = "SELECT TOP 1 GameID FROM dbo.GameTable ORDER BY GameID DESC;";
        command.CommandText = cmdText;
        SqlDataReader reader = command.ExecuteReader();
        if (reader.Read())
        {
            gameID = reader.GetInt32(0);
            currentGameID = gameID;
        }
        reader.Close();
        return gameID;
    }

    /// <summary>
    /// Records the end of a game session by inserting a row into the GameTable.
    /// </summary>
    public void EndGame()
    {
        DateTime endTime = DateTime.Now;

        // Ensure we have an open connection
        this.connection.Open();

        string at = "@";
        string cmdText = $"UPDATE dbo.GameTable SET EndTime = {at}EndTime WHERE GameID = @GameID;";
        using var command = new SqlCommand(cmdText, this.connection);

        var pEnd = command.Parameters.Add("@EndTime", System.Data.SqlDbType.DateTime);
        pEnd.Value = endTime;

        var TEST = command.Parameters.Add("@GameID", SqlDbType.Int);
        TEST.Value = currentGameID;

        _ = command.ExecuteNonQuery();
        this.connection.Close();
    }
}
