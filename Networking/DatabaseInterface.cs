// <copyright file="DatabaseInterface.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System.Data;
using CS3500.Networking.Records;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace CS3500.Networking;

/// <summary>
///     A database interface for the Snake game, stores all game start and end times, along with high scores for every
///     player in every game.
/// </summary>
public class DatabaseInterface
{
    /// <summary>
    /// A string constant for the "@" symbol used in SQL parameters.
    /// A note on this, For some reason, using "@" directly in the SQL strings causes issues with certain analyzers,
    /// and would not let me run the code, this was the only way I could get around that and used parameterized strings.
    /// </summary>
    private static readonly string At = "@";

    /// <summary>
    /// A SQL connection to the remote database.
    /// </summary>
    private readonly SqlConnection connection;

    /// <summary>
    /// The current game ID for the ongoing game session.
    /// </summary>
    private int currentGameId = -1;

    /// <summary>
    /// The start time of the current game session.
    /// </summary>
    private DateTime startTime;

    /// <summary>
    ///     Initializes a new instance of the <see cref="DatabaseInterface" /> class.
    /// </summary>
    public DatabaseInterface()
    {
        // Build configuration to access user secrets
        ConfigurationBuilder builder = new();

        // Bind user secrets to this assembly/type to resolve the generic type symbol
        builder.AddUserSecrets<DatabaseInterface>();
        IConfigurationRoot configuration = builder.Build();
        IConfigurationSection selectedSecrets = configuration.GetSection("SnakeSecrets");

        // Build the connection string using the retrieved secrets
        string connectionString = new SqlConnectionStringBuilder
        {
            DataSource = "cs3500.eng.utah.edu, 14330",
            InitialCatalog = selectedSecrets[ "DB_Name" ],
            UserID = selectedSecrets[ "Username" ],
            Password = selectedSecrets[ "Password" ],
            ConnectTimeout = 15,
            Encrypt = false,
        }.ConnectionString;

        // Initialize and open the SQL connection
        connection = new SqlConnection(connectionString);
    }

    /// <summary>
    ///     Records the end of a game session by updating EndTime for the current game.
    /// </summary>
    public void EndGame()
    {
        // Get the current time as the end time
        DateTime endTime = DateTime.Now;

        EnsureOpenConnection();

        // Build command to update the EndTime for the current game
        string updateGamesTable = $"UPDATE dbo.GameTable SET EndTime = {At}EndTime WHERE GameId = @GameId";
        using SqlCommand gameCommand = new(updateGamesTable, connection);

        // Add parameters and execute the update
        gameCommand.Parameters.Add("@EndTime", SqlDbType.DateTime).Value = endTime;
        gameCommand.Parameters.Add("@GameId", SqlDbType.Int).Value = currentGameId;
        gameCommand.ExecuteNonQuery();

        // Now update all players who have not left yet
        // Start by getting all player IDs in the current game for players who have not left
        string playersToUpdate = "SELECT PlayerId FROM dbo.Players WHERE GameId = @GameId AND LeaveTime IS NULL";
        using SqlCommand selectCommand = new(playersToUpdate, connection);

        // Add parameters and execute the query.
        selectCommand.Parameters.Add("@GameId", SqlDbType.Int).Value = currentGameId;
        using SqlDataReader reader = selectCommand.ExecuteReader();

        // Collect all player IDs who have not left
        List<int> players = new();
        while (reader.Read())
        {
            players.Add(reader.GetInt32(0));
        }

        // Close the reader and update their with a helper method
        reader.Close();
        PlayersLeft(players, endTime);

        // Finally, close the connection
        connection.Close();
    }

    /// <summary>
    /// Gets all games from the database.
    /// </summary>
    /// <returns>A List of GameData objects, containing all fields from the database.</returns>
    public List<GameData> GetAllGames()
    {
        // Initialize a list to hold the game data
        List<GameData> games = new();
        EnsureOpenConnection();
        SqlCommand command = new("SELECT * FROM GameTable", connection);
        using SqlDataReader reader = command.ExecuteReader();

        // Read each record and populate the list
        while (reader.Read())
        {
            int gameId = reader.GetInt32(0);
            DateTime retrievedStartTime = reader.GetDateTime(1);
            DateTime retrievedEndTime = reader.IsDBNull(2) ? DateTime.MinValue : reader.GetDateTime(2);

            // Create new GameData object and add it to the list
            games.Add(new GameData(gameId, retrievedStartTime, retrievedEndTime));
        }

        return games;
    }

    /// <summary>
    /// Gets a single game's player data from the database using the provided game ID.
    /// </summary>
    /// <param name="gameId">The id of the game to get the data from.</param>
    /// <returns>A list of PlayerData objects that contains all fields present in the SQL table.</returns>
    public List<PlayerData> GetSingleGame(int gameId)
    {
        // Initialize a list to hold the player data
        List<PlayerData> players = new();
        EnsureOpenConnection();
        Console.WriteLine("Connection to Database opened.");

        // Prepare the SQL command to retrieve player data for the specified game ID
        SqlCommand command = new(
            "SELECT PlayerId, Name, MaxScore, EnterTime, LeaveTime FROM Players WHERE GameId = @GameId ORDER BY MaxScore DESC",
            connection);

        // Add the game ID parameter to the command
        command.Parameters.Add("@GameId", SqlDbType.Int).Value = gameId;
        using SqlDataReader reader = command.ExecuteReader();
        while (reader.Read())
        {
            // Read all fields for each player and add to the list
            int playerId = reader.GetInt32(0);
            string playerName = reader.GetString(1);
            int maxScore = reader.GetInt32(2);
            DateTime enterTime = reader.GetDateTime(3);
            DateTime leaveTime = reader.IsDBNull(4) ? DateTime.MinValue : reader.GetDateTime(4);

            // Create new PlayerData object and add it to the list
            players.Add(new PlayerData(playerId, gameId, playerName, maxScore, enterTime, leaveTime));
        }

        return players;
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

        // Build the insert command
        string insertSql =
            $"INSERT INTO dbo.Players (GameId, PlayerId, Name, MaxScore, EnterTime) VALUES ({At}GameId, {At}PlayerId, {At}Name, {At}MaxScore, {At}EnterTime)";
        using SqlCommand command = new(insertSql, connection);

        // Add parameters and execute the insert
        command.Parameters.Add("@GameId", SqlDbType.Int).Value = currentGameId; // Use current game ID, stored in this object.
        command.Parameters.Add("@PlayerId", SqlDbType.Int).Value = playerId;
        command.Parameters.Add("@Name", SqlDbType.VarChar).Value = name;
        command.Parameters.Add("@MaxScore", SqlDbType.Int).Value = score;
        command.Parameters.Add("@EnterTime", SqlDbType.DateTime).Value = DateTime.Now; // Use current time as enter time.
        command.ExecuteNonQuery();
    }

    /// <summary>
    ///     Marks the start of a game session.
    /// </summary>
    public void NewGame()
    {
        startTime = DateTime.Now;

        EnsureOpenConnection();

        // Build the insert command to create a new game record, capturing the inserted GameId with OUTPUT.
        string insertSql = $"INSERT INTO dbo.GameTable (StartTime) OUTPUT INSERTED.GameId VALUES ({At}StartTime)";
        using SqlCommand command = new(insertSql, connection);

        // Add parameters and execute the insert
        command.Parameters.Add("@StartTime", SqlDbType.DateTime2).Value = startTime;
        object? scalar = command.ExecuteScalar();

        // Store the new game ID for future operations
        currentGameId = Convert.ToInt32(scalar);
    }

    /// <summary>
    /// Marks the time when a player leaves.
    /// </summary>
    /// <param name="playerId"> The ID of the player that is leaving.</param>
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
        // Only update if the new score is higher than the old score.
        if (newScore <= oldScore)
        {
            return;
        }

        EnsureOpenConnection();

        // Build the update command
        string updateSql =
            $"UPDATE dbo.Players SET MaxScore = {At}MaxScore WHERE GameId = {At}GameId AND PlayerId = {At}PlayerId";
        using SqlCommand command = new(updateSql, connection);

        // Add parameters and execute the update
        command.Parameters.Add("@MaxScore", SqlDbType.Int).Value = newScore;
        command.Parameters.Add("@GameId", SqlDbType.Int).Value = currentGameId;
        command.Parameters.Add("@PlayerId", SqlDbType.Int).Value = playerId;
        command.ExecuteNonQuery();
    }

    /// <summary>
    ///    Ensures that the SQL connection is open, called before any database operation.
    /// </summary>
    private void EnsureOpenConnection()
    {
        if (connection.State != ConnectionState.Open)
        {
            connection.Open();
        }
    }

    /// <summary>
    /// Updates the database to mark when a player has left the game.
    /// </summary>
    /// <param name="playerId">The id of the player who left.</param>
    /// <param name="endTime">The time the player left.</param>
    private void PlayerLeft(int playerId, DateTime endTime)
    {
        EnsureOpenConnection();

        // Build the update command
        string updateSql =
            $"UPDATE dbo.Players SET LeaveTime = {At}LeaveTime WHERE GameId = {At}GameId AND PlayerId = {At}PlayerId";
        using SqlCommand command = new(updateSql, connection);

        // Add parameters and execute the update
        command.Parameters.Add("@LeaveTime", SqlDbType.DateTime2).Value = endTime;
        command.Parameters.Add("@GameId", SqlDbType.Int).Value = currentGameId;
        command.Parameters.Add("@PlayerId", SqlDbType.Int).Value = playerId;

        command.ExecuteNonQuery();
    }

    /// <summary>
    /// A helper method to mark multiple players as having left at the same time.
    /// Used when a game ends to mark all remaining players as having left.
    /// </summary>
    /// <param name="players">A list of player Ids who have not already left the game.</param>
    /// <param name="endTime">THe time the game ended/players left.</param>
    private void PlayersLeft(List<int> players, DateTime endTime)
    {
        foreach (int playerId in players)
        {
            PlayerLeft(playerId, endTime);
        }
    }
}
