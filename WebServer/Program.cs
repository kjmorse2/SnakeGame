// <copyright file="Program.cs" company="PlaceholderCompany">
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
    /// A marker string in the HTML templates where rows will be inserted.
    /// </summary>
    private const string rowInsertionMarker = "<!--ROWS-->";
    /// <summary>
    /// The base address for the server.
    /// </summary>
    private const string address = "http://localhost:";
    /// <summary>
    /// The HTML table data start tag.
    /// </summary>
    private const string tdStart = "<td>";
    /// <summary>
    /// The HTML table data end tag.
    /// </summary>
    private const string tdEnd = "</td>";
    /// <summary>
    /// The HTML table row start tag.
    /// </summary>
    private const string trStart = "<tr>";
    /// <summary>
    /// The HTML table row end tag.
    /// </summary>
    private const string trEnd = "</tr>";
    /// <summary>
    /// The path to the wwwroot directory containing HTML files.
    /// </summary>
    private static readonly string wwwrootPath = Path.Combine(AppContext.BaseDirectory, "wwwroot");
    /// <summary>
    /// The path to the games HTML file.
    /// </summary>
    private static readonly string gamesFilePath = Path.Combine(wwwrootPath, "games.html");
    /// <summary>
    /// The path to the index (home page) HTML file.
    /// </summary>
    private static readonly string indexFilePath = Path.Combine(wwwrootPath, "index.html");
    /// <summary>
    /// The path to the single game HTML file.
    /// </summary>
    private static readonly string singleGameFilePath = Path.Combine(wwwrootPath, "singleGame.html");
    /// <summary>
    /// The shared database interface instance used for data retrieval.
    /// </summary>
    private static readonly DatabaseInterface dbInterface = new();
    /// <summary>
    ///     Shared logger instance used throughout the server for structured logging.
    /// </summary>
    private static ILogger serverLogger = null!;

    /// <summary>
    /// Gets the bytes for the games page, dynamically generating the table rows.
    /// </summary>
    /// <returns>A byte array containing all text from template and database of all games.</returns>
    private static byte[ ] GamesPageBytes()
    {
        try
        {
            // Read the template file
            string template = File.ReadAllText(gamesFilePath, Encoding.UTF8);
            StringBuilder tableBuilder = new(); // Table builder for the full page
            // Find the marker index to insert rows
            int markerIndex = template.IndexOf(rowInsertionMarker, StringComparison.Ordinal);

            // If marker not found, log error and return error page
            if (markerIndex == -1)
            {
                serverLogger.LogError("Row insertion marker not found in games template");
                return "<html><body><h1>Error: Template misconfigured</h1></body></html>"u8.ToArray();
            }

            // Get text before marker and append to builder
            string beginning = template.Substring(0, markerIndex);
            tableBuilder.Append(beginning);

            // Retrieve all games from the database using GameData objects
            List<GameData> games = dbInterface.GetAllGames();

            foreach (GameData game in games)
            {
                // Append each game row to the table
                tableBuilder.Append(MakeGameRow(game));
            }

            // Get text after marker and append to builder
            string end = template.Substring(markerIndex + rowInsertionMarker.Length);
            tableBuilder.Append(end);

            byte[ ] allGameBytes = Encoding.UTF8.GetBytes(tableBuilder.ToString());
            return allGameBytes;
        }
        catch (Exception ex)
        {
            // If any error occurs, log it and return an error page
            serverLogger.LogError(ex, "Error generating games page");
            return "<html><body><h1>Error loading games</h1></body></html>"u8.ToArray();
        }
    }

    /// <summary>
    /// Method that is called when a new connection is established inside ServerConnection.
    /// </summary>
    /// <param name="context">The context object returned when the HTTP Listener establishes a connection.</param>
    private static void HandleConnect(HttpListenerContext context)
    {
        serverLogger.LogTrace("Connection established with : " + context);

        try
        {
            string method = context.Request.HttpMethod;
            // The buffer to hold the response (webpage) data
            byte[ ] buffer;

            // Ensure we only handle GET requests
            if (method == "GET")
            {
                // Get the requested URL, and later its path
                Uri? url = context.Request.Url;

                if (url == null)
                {
                    serverLogger.LogWarning("Received request with null URL");
                    SendErrorResponse(context, 400, "Bad Request");
                    return;
                }

                serverLogger.LogInformation("Received GET request for URL: " + url);
                // Get the absolute path from the URL
                string path = url.AbsolutePath;

                // Convert the path into a list of strings, removing empty entries
                List<string> components = path.Split('/', StringSplitOptions.RemoveEmptyEntries).ToList();

                // If the size is greater than 0, and the first component is "games", we know we are not headed to homepage.
                if (components.Count > 0 && components[ 0 ] == "games")
                {
                    // If there are two components, and the second is an integer, we serve single game page
                    if (components.Count == 2 && int.TryParse(components[ 1 ], out int gameId))
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
            serverLogger.LogError(ex, "Error handling connection");
            try
            {
                SendErrorResponse(context, 500, "Internal Server Error");
            }
            catch
            {
                // If we can't send error response, just log it
                serverLogger.LogError("Failed to send error response");
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
                serverLogger.LogError(ex, "Error closing response stream");
            }
        }
    }

    /// <summary>
    /// Reads and returns the bytes for the home page.
    /// </summary>
    /// <returns>A byte array of all the index.html chracters</returns>
    private static byte[ ] HomePageBytes()
    {
        try
        {
            return File.ReadAllBytes(indexFilePath);
        }
        catch (Exception ex)
        {
            serverLogger.LogError(ex, "Error reading home page file");
            return "<html><body><h1>Error loading home page</h1></body></html>"u8.ToArray();
        }
    }

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
        try
        {
            string template = File.ReadAllText(singleGameFilePath, Encoding.UTF8);
            StringBuilder tableBuilder = new();
            int markerIndex = template.IndexOf(rowInsertionMarker, StringComparison.Ordinal);
            
            if (markerIndex == -1)
            {
                serverLogger.LogError("Row insertion marker not found in single game template");
                return "<html><body><h1>Error: Template misconfigured</h1></body></html>"u8.ToArray();
            }

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
        catch (Exception ex)
        {
            serverLogger.LogError(ex, "Error generating single game page for game ID: {GameId}", gameId);
            return "<html><body><h1>Error loading game details</h1></body></html>"u8.ToArray();
        }
    }
}
