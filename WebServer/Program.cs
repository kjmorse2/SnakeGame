// <copyright file="ChatServer.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System.Collections.Concurrent;
using System.Net;
using CS3500.Networking;
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
            string responseString = "<html><body><h1>Hello, World!</h1></body></html>";
            byte[] buffer = System.Text.Encoding.UTF8.GetBytes( responseString );
            context.Response.StatusCode = 200;
            context.Response.ContentLength64 = buffer.Length;
            context.Response.OutputStream.Write( buffer, 0, buffer.Length );
            context.Response.OutputStream.Close();
        }


    }
}