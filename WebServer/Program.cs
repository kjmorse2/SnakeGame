// <copyright file="ChatServer.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System.Net;
using System.Text;
using CS3500.Networking;
using CS3500.Networking.Records;
using Microsoft.Extensions.Logging;

namespace CS3500.SnakeServer;

/// <summary>
///     A simple ChatServer that handles clients separately and replies with a static message.
/// </summary>
public class SnakeServer
{
    private const string rowInsertionMarker = "<!--Rows-->";
    private static readonly string wwwrootPath = Path.Combine(AppContext.BaseDirectory, "wwwroot");
    private static readonly string gameFilePath = Path.Combine(wwwrootPath, "game.html");
    private static readonly string gamesFilePath = Path.Combine(wwwrootPath, "games.html");
    private static readonly string indexFilePath = Path.Combine(wwwrootPath, "index.html");
    private static readonly string singleGameFilePath = Path.Combine(wwwrootPath, "singleGame.html");


    private static readonly DatabaseInterface dbInterface = new();

    /// <summary>
    ///     Holds all the currently connected clients.
    ///     The key is the client's username, and the value is the corresponding NetworkConnection object.
    ///     A ConcurrentDictionary ensures thread-safe access.
    /// </summary>
    /// <summary>
    ///     Shared logger instance used throughout the server for structured logging.
    /// </summary>
    private static ILogger serverLogger;

    private static byte[ ] GamesPageBytes()
    {
        string template = File.ReadAllText(gamesFilePath, Encoding.UTF8);
        StringBuilder tableBuilder = new();
        int markerIndex = template.IndexOf(rowInsertionMarker, StringComparison.Ordinal);
        string beginning = template.Substring(0, markerIndex);
        tableBuilder.Append(beginning);
        List<GameData> games = dbInterface.GetAllGames();

        foreach (GameData game in games)
        {
            tableBuilder.Append(MakeRow(game.ToStringList()));
        }

        string end = template.Substring(markerIndex + rowInsertionMarker.Length);
        tableBuilder.Append(end);

        byte[ ] allGameBytes = Encoding.UTF8.GetBytes(tableBuilder.ToString());
        return allGameBytes;
    }

    private static void HandleConnect(HttpListenerContext context)
    {
        serverLogger.LogTrace("Connection established with : " + context);

        string method = context.Request.HttpMethod;
        if (method == "GET")
        {
            Uri? url = context.Request.Url;
            serverLogger.LogInformation("Received GET request for URL: " + url);
            // Handle GET request

            byte[ ] buffer = SingleGamePageBytes();
            context.Response.StatusCode = 200;
            context.Response.ContentType = "text/html";
            context.Response.ContentLength64 = buffer.Length;
            context.Response.OutputStream.Write(buffer, 0, buffer.Length);
            context.Response.OutputStream.Close();
        }
    }

    private static byte[ ] HomePageBytes()
    {
        return File.ReadAllBytes(indexFilePath);
    }

    private static void Main()
    {
        using ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddConsole(); // JIM: must nuget add Microsoft.Extensions.Logging.Console and Debug
            builder.AddDebug();
            builder.SetMinimumLevel(LogLevel.Trace);
        });

        serverLogger = loggerFactory.CreateLogger<SnakeServer>();
        serverLogger.LogInformation("Server initialized, waiting for connections...");

        ServerConnection.WaitForConnections(HandleConnect, 8080, serverLogger);
        Console.Read(); // don't stop the program.
    }

    private static string MakeRow(List<string> elements)
    {
        const string tdStart = "<td>";
        const string tdEnd = "</td>";
        const string trStart = "<tr>";
        const string trEnd = "</tr>";
        StringBuilder rowBuilder = new();

        rowBuilder.Append(trStart);
        foreach (string element in elements)
        {
            rowBuilder.Append(tdStart);
            rowBuilder.Append(element);
            rowBuilder.Append(tdEnd);
        }

        rowBuilder.Append(trEnd);

        return rowBuilder.ToString();
    }

    private static byte[ ] SingleGamePageBytes()
    {
        string template = File.ReadAllText(singleGameFilePath, Encoding.UTF8);
        StringBuilder tableBuilder = new();
        int markerIndex = template.IndexOf(rowInsertionMarker, StringComparison.Ordinal);
        string beginning = template.Substring(0, markerIndex);
        tableBuilder.Append(beginning);
        List<PlayerData> players = dbInterface.GetSingleGame(20);

        foreach (PlayerData player in players)
        {
            tableBuilder.Append(MakeRow(player.ToStringList()));
        }

        string end = template.Substring(markerIndex + rowInsertionMarker.Length);
        tableBuilder.Append(end);

        byte[ ] allGameBytes = Encoding.UTF8.GetBytes(tableBuilder.ToString());
        return allGameBytes;
    }
}
