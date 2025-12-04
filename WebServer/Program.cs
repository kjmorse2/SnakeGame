// <copyright file="ChatServer.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System.Net;
using System.Text;
using CS3500.Networking;
using CS3500.Networking.Records;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualBasic.CompilerServices;

namespace CS3500.SnakeServer;

/// <summary>
///     A simple ChatServer that handles clients separately and replies with a static message.
/// </summary>
public class SnakeServer
{
    private const string rowInsertionMarker = "<!--ROWS-->";
    private const string address = "http://localhost:";
    private const string tdStart = "<td>";
    private const string tdEnd = "</td>";
    private const string trStart = "<tr>";
    private const string trEnd = "</tr>";
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
            tableBuilder.Append(MakeGameRow(game));
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
        byte[ ] buffer;
        if (method == "GET")
        {
            Uri? url = context.Request.Url;
            serverLogger.LogInformation("Received GET request for URL: " + url);
            string path = url.AbsolutePath;
            List<string> components = path.Split('/').ToList();
            for (int i = 0; i < components.Count; i++)
            {
                if (components[ i ].IsNullOrEmpty())
                {
                    components.RemoveAt(i);
                }
            }

            if (components.Count > 0 && components[ 0 ] == "games")
            {
                if (components.Count == 2 && int.TryParse(components[ 1 ], out int gameId))
                {
                    buffer = SingleGamePageBytes(gameId);
                }
                else
                {
                    buffer = GamesPageBytes();
                }
            }
            else
            {
                buffer = HomePageBytes();
            }

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

        ServerConnection.WaitForConnections(HandleConnect, address, 8080, serverLogger);
        Console.Read(); // don't stop the program.
    }

    private static string MakePlayerRow(PlayerData player)
    {
        StringBuilder rowBuilder = new();
        List<string> elements = player.ToStringList();

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

    private static string MakeGameRow(GameData game)
    {
        StringBuilder rowBuilder = new();
        List<string> elements = game.ToStringList();
        rowBuilder.Append(trStart);
        rowBuilder.Append(tdStart);
        rowBuilder.Append("<a href=\"games/" + game.GameId + "\">" + game.GameId + "</a>");
        rowBuilder.Append(tdEnd);
        for (int i = 1; i < elements.Count; i++)
        {
            rowBuilder.Append(tdStart);
            rowBuilder.Append(elements[i]);
            rowBuilder.Append(tdEnd);
        }
        rowBuilder.Append(trEnd);
        return rowBuilder.ToString();
    }

    private static byte[ ] SingleGamePageBytes(int gameId)
    {
        string template = File.ReadAllText(singleGameFilePath, Encoding.UTF8);
        StringBuilder tableBuilder = new();
        int markerIndex = template.IndexOf(rowInsertionMarker, StringComparison.Ordinal);
        string beginning = template.Substring(0, markerIndex);
        tableBuilder.Append(beginning);
        List<PlayerData> players = dbInterface.GetSingleGame(gameId);

        foreach (PlayerData player in players)
        {
            tableBuilder.Append(MakePlayerRow(player));
        }

        string end = template.Substring(markerIndex + rowInsertionMarker.Length);
        tableBuilder.Append(end);

        byte[ ] allGameBytes = Encoding.UTF8.GetBytes(tableBuilder.ToString());
        return allGameBytes;
    }
}
