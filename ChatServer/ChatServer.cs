// <copyright file="ChatServer.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System.Collections.Concurrent;
using CS3500.Networking;
using Microsoft.Extensions.Logging;

namespace CS3500.Chatting;

/// <summary>
///   A simple ChatServer that handles clients separately and replies with a static message.
/// </summary>
public partial class ChatServer
{
    /// <summary>
    /// Holds all the currently connected clients.
    /// The key is the client's username, and the value is the corresponding NetworkConnection object.
    /// A ConcurrentDictionary ensures thread-safe access.
    /// </summary>
    private static ConcurrentDictionary<string, NetworkConnection> connectedClients = new();

    /// <summary>
    /// Shared logger instance used throughout the server for structured logging.
    /// </summary>
    private static ILogger serverLogger;

    /// <summary>
    ///   The main program.
    /// </summary>
    private static void Main()
    {
        using var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddConsole(); // JIM: must nuget add Microsoft.Extensions.Logging.Console and Debug
            builder.AddDebug();
            builder.SetMinimumLevel( LogLevel.Trace );
        } );

        serverLogger = loggerFactory.CreateLogger<ChatServer>();
        serverLogger.LogInformation("Server initialized, waiting for connections...");

        ServerConnection.WaitForConnections( HandleConnect, 11_000, serverLogger );
        Console.Read(); // don't stop the program.
    }

    /// <summary>
    ///   <para>
    ///     Current (Wrong) Functionality: When a new connection is established,
    ///     enter a loop that receives from the client and sends "thanks" back
    ///     to the client for each message received.
    ///   </para>
    ///   <para>
    ///     TODO: Expected functionality: When a new connection is established:
    ///   </para>
    ///   <list type="number">
    ///     <item>
    ///       Read the name of the connection and store it.
    ///     </item>
    ///     <item>
    ///       Begin a loop that reads messages from the client.
    ///     </item>
    ///     <item>
    ///       For each message received, broadcast that message to all connected clients,
    ///     </item>
    ///     <item>
    ///       If the client disconnects (this will throw an exception), remove them from the list of connected clients.
    ///     </item>
    ///   </list>
    ///   <para>
    ///     All actions on the list of connected clients must be thread-safe.
    ///   </para>
    ///   <para>
    ///     All important events (connections, disconnections, messages received, messages sent, errors, etc.)
    ///     must be logged using the logging system at the appropriate log level.
    ///   </para>
    /// </summary>
    private static void HandleConnect( NetworkConnection connection )
    {
        var name = connection.ReceiveLine();
        serverLogger.LogInformation( "Connection established, name received: " + name );
        connectedClients[ name ] = connection;

        try
        {
            while ( true )
            {
                var message = connection.ReceiveLine();
                serverLogger.LogDebug("Received message from " + name + ": " + message);
                foreach ( var sendName in connectedClients.Keys)
                {
                    var connectedClient = connectedClients[ sendName ];
                    string fullMessage = $"{name}: {message}";
                    serverLogger.LogTrace($"Sending \"{fullMessage}\" to {sendName}.");
                    connectedClient.SendLine( fullMessage );
                }
            }
        }
        catch ( Exception )
        {
            serverLogger.LogInformation($"Connection with {name} has been lost.");
            connection.Dispose();
            connectedClients.Remove( name, out _ );
        }
    }
}
