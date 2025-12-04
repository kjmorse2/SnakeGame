﻿// <copyright file="Program.cs" company="PlaceholderCompany">
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
    /// <summary>
    ///     The base address for the server.
    /// </summary>
    private const string Address = "http://localhost:";

    /// <summary>
    ///     A marker string in the HTML templates where rows will be inserted.
    /// </summary>
    private const string RowInsertionMarker = "<!--ROWS-->";

    /// <summary>
    ///     The HTML table data end tag.
    /// </summary>
    private const string TdEnd = "</td>";

    /// <summary>
    ///     The HTML table data start tag.
    /// </summary>
    private const string TdStart = "<td>";

    /// <summary>
    ///     The HTML table row end tag.
    /// </summary>
    private const string TrEnd = "</tr>";

    /// <summary>
    ///     The HTML table row start tag.
    /// </summary>
    private const string TrStart = "<tr>";

    /// <summary>
    ///     The shared database interface instance used for data retrieval.
    /// </summary>
    private static readonly DatabaseInterface DbInterface = new();

    /// <summary>
    ///     The path to the wwwroot directory containing HTML files.
    /// </summary>
    private static readonly string WwwrootPath = Path.Combine(AppContext.BaseDirectory, "wwwroot");

    /// <summary>
    ///     The path to the games HTML file.
    /// </summary>
    private static readonly string GamesFilePath = Path.Combine(WwwrootPath, "games.html");

    /// <summary>
    ///     The path to the index (home page) HTML file.
    /// </summary>
    private static readonly string IndexFilePath = Path.Combine(WwwrootPath, "index.html");

    /// <summary>
    ///     The path to the single game HTML file.
    /// </summary>
    private static readonly string SingleGameFilePath = Path.Combine(WwwrootPath, "singleGame.html");

    /// <summary>
    ///     Shared logger instance used throughout the server for structured logging.
    /// </summary>
    private static ILogger ServerLogger = null!;

    /// <summary>
    ///     Gets the bytes for the games page, dynamically generating the table rows.
    /// </summary>
    /// <returns>A byte array containing all text from template and database of all games.</returns>
    private static byte[ ] GamesPageBytes()
    {
        try
        {
            // Read the template file
            string template = File.ReadAllText(GamesFilePath, Encoding.UTF8);
            StringBuilder tableBuilder = new(); // Table builder for the full page

            // Find the marker index to insert rows
            int markerIndex = template.IndexOf(RowInsertionMarker, StringComparison.Ordinal);

            // If marker not found, log error and return error page
            if (markerIndex == -1)
            {
                ServerLogger.LogError("Row insertion marker not found in games template");
                return "<html><body><h1>Error: Template misconfigured</h1></body></html>"u8.ToArray();
            }

            // Get text before marker and append to builder
            string beginning = template.Substring(0, markerIndex);
            tableBuilder.Append(beginning);

            // Retrieve all games from the database using GameData objects
            List<GameData> games = DbInterface.GetAllGames();

            foreach (GameData game in games)
            {
                // Append each game row to the table
                tableBuilder.Append(MakeGameRow(game));
            }

            // Get text after marker and append to builder
            string end = template.Substring(markerIndex + RowInsertionMarker.Length);
            tableBuilder.Append(end);

            byte[ ] allGameBytes = Encoding.UTF8.GetBytes(tableBuilder.ToString());
            return allGameBytes;
        }
        catch (Exception ex)
        {
            // If any error occurs, log it and return an error page
            ServerLogger.LogError(ex, "Error generating games page");
            return "<html><body><h1>Error loading games</h1></body></html>"u8.ToArray();
        }
    }

    /// <summary>
    ///     Method that is called when a new connection is established inside ServerConnection.
    /// </summary>
    /// <param name="context">The context object returned when the HTTP Listener establishes a connection.</param>
    private static void HandleConnect(HttpListenerContext context)
    {
        ServerLogger.LogTrace("Connection established with : " + context);

        try
        {
            string method = context.Request.HttpMethod;

            // The buffer to hold the response (webpage) data

            // Ensure we only handle GET requests
            if (method == "GET")
            {
                // Get the requested URL, and later its path
                Uri? url = context.Request.Url;

                if (url == null)
                {
                    ServerLogger.LogWarning("Received request with null URL");
                    SendErrorResponse(context, 400, "Bad Request");
                    return;
                }

                ServerLogger.LogInformation("Received GET request for URL: " + url);

                // Get the absolute path from the URL
                string path = url.AbsolutePath;

                // Convert the path into a list of strings, removing empty entries
                List<string> components = path.Split('/', StringSplitOptions.RemoveEmptyEntries).ToList();

                // The buffer to hold the response data
                byte[ ] buffer;

                // If the size is greater than 0, and the first component is "games", we know we are not headed to homepage.
                if (components.Count > 0 && components[ 0 ] == "games")
                {
                    // Check if there's a query string parameter "gid"
                    string? gidParam = null;
                    if (!string.IsNullOrEmpty(url.Query))
                    {
                        // Parse query string manually (e.g., "?gid=123")
                        string query = url.Query.TrimStart('?');
                        string[ ] parameters = query.Split('&');
                        foreach (string param in parameters)
                        {
                            string[ ] keyValue = param.Split('=');
                            if (keyValue.Length == 2 && keyValue[ 0 ] == "gid")
                            {
                                gidParam = keyValue[ 1 ];
                                break;
                            }
                        }
                    }

                    // If gid parameter exists and is a valid integer, serve single game page
                    if (gidParam != null && int.TryParse(gidParam, out int gameId))
                    {
                        buffer = SingleGamePageBytes(gameId);
                    }

                    // Otherwise, we serve the general games page
                    else
                    {
                        buffer = GamesPageBytes();
                    }
                }

                // If no specific path, or malformed path, serve the home page
                else
                {
                    buffer = HomePageBytes();
                }

                // Configure and send the response
                context.Response.StatusCode = 200;
                context.Response.ContentType = "text/html";
                context.Response.ContentLength64 = buffer.Length;
                context.Response.OutputStream.Write(buffer, 0, buffer.Length);
            }
            else
            {
                // If method is not GET, respond with 405 Method Not Allowed
                SendErrorResponse(context, 405, "Method Not Allowed");
            }
        }
        catch (Exception ex)
        {
            // If any error occurs during handling, log it and send a 500 Internal Server Error response
            ServerLogger.LogError(ex, "Error handling connection");
            try
            {
                SendErrorResponse(context, 500, "Internal Server Error");
            }
            catch
            {
                // If we can't send error response, just log it
                ServerLogger.LogError("Failed to send error response");
            }
        }

        // In all cases, we close the response stream
        finally
        {
            try
            {
                context.Response.OutputStream.Close();
            }
            catch (Exception ex)
            {
                ServerLogger.LogError(ex, "Error closing response stream");
            }
        }
    }

    /// <summary>
    ///     Reads and returns the bytes for the home page.
    /// </summary>
    /// <returns>A byte array of all the index.html characters.</returns>
    private static byte[ ] HomePageBytes()
    {
        try
        {
            return File.ReadAllBytes(IndexFilePath);
        }
        catch (Exception ex)
        {
            ServerLogger.LogError(ex, "Error reading home page file");
            return "<html><body><h1>Error loading home page</h1></body></html>"u8.ToArray();
        }
    }

    /// <summary>
    ///     The main entry point for the SnakeServer application.
    /// </summary>
    private static void Main()
    {
        // Set up a logger
        using ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddConsole(); // JIM: must nuget add Microsoft.Extensions.Logging.Console and Debug
            builder.AddDebug();
            builder.SetMinimumLevel(LogLevel.Trace);
        });

        ServerLogger = loggerFactory.CreateLogger<SnakeServer>();
        ServerLogger.LogInformation("Server initialized, waiting for connections...");

        // Start the main server loop to wait for connections, Handle connect is called on each connection.
        ServerConnection.WaitForConnections(HandleConnect, Address, 8080, ServerLogger);
        Console.Read(); // don't stop the program.
    }

    /// <summary>
    ///     Convert a GameData object into an HTML table row, with a link on the Game ID.
    /// </summary>
    /// <param name="game">The GameData object to convert into a row.</param>
    /// <returns>A string containing the HTML for a GameData row, with link on Game ID.</returns>
    private static string MakeGameRow(GameData game)
    {
        // StringBuilder for constructing the row
        StringBuilder rowBuilder = new();

        // Convert game data to list of strings
        List<string> elements = game.ToStringList();

        // Build first division manually with link on Game ID using query string format
        rowBuilder.Append(TrStart);
        rowBuilder.Append(TdStart);
        rowBuilder.Append("<a href=\"games?gid=" + game.GameId + "\">" + game.GameId + "</a>");
        rowBuilder.Append(TdEnd);

        // Build the rest of the row
        for (int i = 1; i < elements.Count; i++)
        {
            rowBuilder.Append(TdStart);
            rowBuilder.Append(elements[ i ]);
            rowBuilder.Append(TdEnd);
        }

        rowBuilder.Append(TrEnd);
        return rowBuilder.ToString();
    }

    /// <summary>
    ///     Converts a PlayerData object into an HTML table row, in a specified order.
    /// </summary>
    /// <param name="player">The player to make a row out of.</param>
    /// <returns>A string containing the HTML for a player row.</returns>
    private static string MakePlayerRow(PlayerData player)
    {
        // StringBuilder for constructing the row
        StringBuilder rowBuilder = new();

        // Convert player data to list of strings
        List<string> elements = player.ToStringList();

        // Build the HTML row
        rowBuilder.Append(TrStart);
        foreach (string element in elements)
        {
            rowBuilder.Append(TdStart);
            rowBuilder.Append(element);
            rowBuilder.Append(TdEnd);
        }

        rowBuilder.Append(TrEnd);

        return rowBuilder.ToString();
    }

    /// <summary>
    ///     Method for sending an error response with given status code and description.
    /// </summary>
    /// <param name="context">The context that produced the error.</param>
    /// <param name="statusCode">The status code produced by the error.</param>
    /// <param name="statusDescription">The description of the code.</param>
    private static void SendErrorResponse(HttpListenerContext context, int statusCode, string statusDescription)
    {
        string errorHtml = $"<html><body><h1>{statusCode} - {statusDescription}</h1></body></html>";
        byte[ ] buffer = Encoding.UTF8.GetBytes(errorHtml);

        context.Response.StatusCode = statusCode;
        context.Response.StatusDescription = statusDescription;
        context.Response.ContentType = "text/html";
        context.Response.ContentLength64 = buffer.Length;
        context.Response.OutputStream.Write(buffer, 0, buffer.Length);
    }

    /// <summary>
    ///     Get the bytes for a single game's page, dynamically generating the player rows.
    /// </summary>
    /// <param name="gameId">The GameId of the game we want to see players from.</param>
    /// <returns>An array of bytes containing all the HTML from the template and generated rows.</returns>
    private static byte[ ] SingleGamePageBytes(int gameId)
    {
        try
        {
            // Read the template file
            string template = File.ReadAllText(SingleGameFilePath, Encoding.UTF8);

            // Replaces %%GameID%% From the HTML file to the actual game ID.
            template = template.Replace("%%GameID%%", gameId.ToString());

            // StringBuilder for the full page
            StringBuilder tableBuilder = new();

            // Find the marker index to insert the table rows from database
            int markerIndex = template.IndexOf(RowInsertionMarker, StringComparison.Ordinal);

            // If marker not found, log error and return error page
            if (markerIndex == -1)
            {
                ServerLogger.LogError("Row insertion marker not found in single game template");
                return "<html><body><h1>Error: Template misconfigured</h1></body></html>"u8.ToArray();
            }

            // Get text before marker and append to builder
            string beginning = template.Substring(0, markerIndex);
            tableBuilder.Append(beginning);

            // Retrieve all players for the specified game from the database using PlayerData objects
            List<PlayerData> players = DbInterface.GetSingleGame(gameId);

            // Append each player row to the table
            foreach (PlayerData player in players)
            {
                tableBuilder.Append(MakePlayerRow(player));
            }

            // Get text after marker and append to builder
            string end = template.Substring(markerIndex + RowInsertionMarker.Length);
            tableBuilder.Append(end);

            // Convert the full page to bytes and return
            byte[ ] allGameBytes = Encoding.UTF8.GetBytes(tableBuilder.ToString());
            return allGameBytes;
        }
        catch (Exception ex)
        {
            // If any error occurs, log it and return an error page
            ServerLogger.LogError(ex, "Error generating single game page for game ID: {GameId}", gameId);
            return "<html><body><h1>Error loading game details</h1></body></html>"u8.ToArray();
        }
    }
}
