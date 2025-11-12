// <copyright file="ChatServer.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System.Collections.Concurrent;
using CS3500.Networking;
using Microsoft.Extensions.Logging;

namespace CS3500.Chatting;

/// <summary>
///     A simple ChatServer that handles clients separately and replies with a static message.
/// </summary>
public class ChatServer
{
    /// <summary>
    ///     Holds all the currently connected clients.
    ///     The key is the client's username, and the value is the corresponding NetworkConnection object.
    ///     A ConcurrentDictionary ensures thread-safe access.
    /// </summary>
    private static readonly ConcurrentDictionary<string, NetworkConnection> ConnectedClients = new();

    /// <summary>
    ///     Shared logger instance used throughout the server for structured logging.
    /// </summary>
    private static ILogger ServerLogger = null!;

    /// <summary>
    ///     <para>
    ///         Current (Wrong) Functionality: When a new connection is established,
    ///         enter a loop that receives from the client and sends "thanks" back
    ///         to the client for each message received.
    ///     </para>
    /// </summary>
    private static void HandleConnect(NetworkConnection connection)
    {
        string name = connection.ReceiveLine();
        ServerLogger.LogInformation("Connection established, name received: " + name);
        ConnectedClients[ name ] = connection;

        try
        {
            while (true)
            {
                string message = connection.ReceiveLine();
                ServerLogger.LogDebug("Received message from " + name + ": " + message);
                foreach (string sendName in ConnectedClients.Keys)
                {
                    NetworkConnection connectedClient = ConnectedClients[ sendName ];
                    string fullMessage = $"{name}: {message}";
                    ServerLogger.LogTrace($"Sending \"{fullMessage}\" to {sendName}.");
                    connectedClient.SendLine(fullMessage);
                }
            }
        }
        catch (Exception)
        {
            ServerLogger.LogInformation($"Connection with {name} has been lost.");
            connection.Dispose();
            ConnectedClients.Remove(name, out _);
        }
    }

    /// <summary>
    ///     The main program.
    /// </summary>
    private static void Main()
    {
        using ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddConsole(); // JIM: must nuget add Microsoft.Extensions.Logging.Console and Debug
            builder.AddDebug();
            builder.SetMinimumLevel(LogLevel.Trace);
        });

        ServerLogger = loggerFactory.CreateLogger<ChatServer>();
        ServerLogger.LogInformation("Server initialized, waiting for connections...");

        ServerConnection.WaitForConnections(HandleConnect, 11_000, ServerLogger);
        Console.Read(); // don't stop the program.
    }
}
