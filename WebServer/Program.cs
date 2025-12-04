// <copyright file="ChatServer.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System.Net;
using System.Text;
using CS3500.Networking;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using CS3500.Networking;

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
        StringBuilder rowsStringBuilder = new StringBuilder();
        string rowsString = rowsStringBuilder.ToString();

        using (SqlConnection sqlConn = new(SnakeSecrets))
        {
            try
            {
                sqlConn.Open();
                Console.WriteLine("Connection to Database opened.");
                SqlCommand command = new("SELECT * FROM GameTable", sqlConn);
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int gameID = reader.GetInt32(0);
                        DateTime start = reader.GetDateTime(1);
                        DateTime? endTime = reader.GetDateTime(2);
                        rowsBuilder.Append("<tr>");
                        sb.Append($"<td><a href=\"/games?gameID={gameID}\">{gameID}</a></td>");
                        sb.Append($"<td>{start} </td>");
                        sb.Append($"<td>{(end.HasValue ? end.Value.ToString() : String.Empty)}</td>");
                        sb.Append("</tr>");
                    }
                }
            }
            catch { }
        }

        string end = template.Substring(markerIndex + rowsString.Length, template.Length);
        byte[ ] allGameBytes = Encoding.UTF8.GetBytes(beginning + rowsString + end);
        //byte[ ] allGameBytes = Encoding.UTF8.GetBytes(template);
        return allGameBytes;
    }

    private static string MakeRow(string rowContents)
    {
        const string tdStart = "<td>";
        const string tdEnd = "</td>";
        return new StringBuilder().Append(tdStart).Append(tdEnd).Append(rowContents).ToString();
    }
}
