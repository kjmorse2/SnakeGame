// <copyright file="ChatServer.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System.Net;
using System.Text;
using CS3500.Networking;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace CS3500.SnakeServer;

/// <summary>
///   A simple ChatServer that handles clients separately and replies with a static message.
/// </summary>
public class SnakeServer
{
    /// <summary>
    /// Holds all the currently connected clients.
    /// The key is the client's username, and the value is the corresponding NetworkConnection object.
    /// A ConcurrentDictionary ensures thread-safe access.
    /// </summary>

    /// <summary>
    /// Shared logger instance used throughout the server for structured logging.
    /// </summary>
    private static ILogger serverLogger;

    /// <summary>
    /// New DatabaseInterface instance used to get the data from the table in the database.
    /// </summary>
    private static DatabaseInterface dbInterface = new();

    private static readonly string wwwrootPath = Path.Combine(AppContext.BaseDirectory, "wwwroot");
    private static readonly string indexFilePath = Path.Combine(wwwrootPath, "index.html");
    private static readonly string gamesFilePath = Path.Combine(wwwrootPath, "games.html");
    private static readonly string rowInsertionMarker = "<!--Rows-->";
    private static readonly string gameFilePath = Path.Combine(wwwrootPath, "game.html");

    private static void Main()
    {
        using var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddConsole(); // JIM: must nuget add Microsoft.Extensions.Logging.Console and Debug
            builder.AddDebug();
            builder.SetMinimumLevel( LogLevel.Trace );
        } );

        serverLogger = loggerFactory.CreateLogger<SnakeServer>();
        serverLogger.LogInformation("Server initialized, waiting for connections...");

        ServerConnection.WaitForConnections( HandleConnect, 8080, serverLogger );
        Console.Read(); // don't stop the program.
    }

    private static void HandleConnect( HttpListenerContext context)
    {
        serverLogger.LogTrace( "Connection established with : " + context.ToString() );

        string method = context.Request.HttpMethod;
        if (method == "GET")
        {
            Uri? url = context.Request.Url;
            serverLogger.LogInformation( "Received GET request for URL: " + url );
            // Handle GET request

            byte[] buffer = GamesPageBytes();
            context.Response.StatusCode = 200;
            context.Response.ContentType = "text/html";
            context.Response.ContentLength64 = buffer.Length;
            context.Response.OutputStream.Write( buffer, 0, buffer.Length );
            context.Response.OutputStream.Close();
        }
    }

    private static byte[ ] HomePageBytes()
    {
        return File.ReadAllBytes(indexFilePath);
    }

    private static byte [ ] GamesPageBytes()
    {
        string template = File.ReadAllText(gamesFilePath, Encoding.UTF8);
        const string rowInsertionMarker = "<!--ROWS-->";
        int markerIndex = template.IndexOf(rowInsertionMarker, StringComparison.Ordinal);
        string beginning = template.Substring(0, markerIndex);
        StringBuilder rowsStringBuilder = new();
        dbInterface.GetAllGames(out List<int> gameIds, out List<string> startTimes, out List<string> endTimes);

        for(int i = 0; i < gameIds.Count; i++)
        {
            List<string> row = new();
            row.Add(gameIds[i] + "");
            row.Add(startTimes[i]);
            row.Add(endTimes[i]);
            rowsStringBuilder.Append(MakeRow(row));
        }

        string rowsString = rowsStringBuilder.ToString();
        string end = template.Substring(markerIndex + rowsString.Length, template.Length);
        byte[ ] allGameBytes = Encoding.UTF8.GetBytes(beginning + rowsString + end);
        //byte[ ] allGameBytes = Encoding.UTF8.GetBytes(template);
        return allGameBytes;
    }

    private static byte[ ] SingleGamePageBytes()
    {
        string template = File.ReadAllText(gamesFilePath, Encoding.UTF8);
        const string rowInsertionMarker = "<!--ROWS-->";
        int markerIndex = template.IndexOf(rowInsertionMarker, StringComparison.Ordinal);
        string beginning = template.Substring(0, markerIndex);
        StringBuilder rowsStringBuilder = new ();
        dbInterface.GetSingleGame(1, out List<int> playerIds, out List<string> playerNames, out List<int> highScores, out List<string> enterTimes, out List<string> leaveTimes);

        for (int i = 0; i < playerIds.Count; i++)
        {
            List<string> row = new();
            row.Add(playerIds[i] + "");
            row.Add(playerNames[i]);
            row.Add(highScores[i] + "");
            row.Add(enterTimes[i]);
            row.Add(leaveTimes[i]);
            rowsStringBuilder.Append(MakeRow(row));
        }

        string rowsString = rowsStringBuilder.ToString();
        string end = template.Substring(markerIndex + rowsString.Length, template.Length);
        byte[] allGameBytes = Encoding.UTF8.GetBytes(beginning + rowsString + end);
        //byte[ ] allGameBytes = Encoding.UTF8.GetBytes(template);
        return allGameBytes;
    }

    private static string MakeRow(List<string> elements)
    {
        const string tdStart = "<td>";
        const string tdEnd = "</td>";
        const string trStart = "<tr>";
        const string trEnd = "</tr>";
        StringBuilder rowBuilder = new();
        rowBuilder.Append(trStart);
        foreach(string element in elements)
        {
            rowBuilder.Append(tdStart);
            rowBuilder.Append(element);
            rowBuilder.Append(tdEnd);
        }
        rowBuilder.Append(trEnd);
        return rowBuilder.ToString();
    }
}
