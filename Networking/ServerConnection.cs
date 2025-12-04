// <copyright file="ServerConnection.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;

namespace CS3500.Networking;

/// <summary>
///     Represents a server task that waits for connections on a given
///     port and calls the provided delegate when a connection is made.
/// </summary>
public static class ServerConnection
{
    /// <summary>
    ///     Use on a TcpListener to handle new connections. Alert the calling program/function
    ///     via the handleConnect delegate.
    /// </summary>
    /// <param name="handleConnect">
    ///     Handler for what the user wants to do when a connection is made.
    ///     This should be run asynchronously via a new thread.
    /// </param>
    /// <param name="port"> The port (e.g., 11000) to listen on. </param>
    /// <param name="logger"> The logger instance used for logging connection events. </param>
    public static void WaitForConnections(Action<HttpListenerContext> handleConnect, int port, ILogger logger)
    {
        HttpListener listener = new();
        listener.Prefixes.Add("http://localhost:" + port + "/");
        listener.Start();
        ILogger localLogger = logger;

        while (true)
        {
            localLogger.LogInformation("Waiting for connections...");
            HttpListenerContext context = listener.GetContext();
            localLogger.LogInformation("Connection found, attempting to connect.");
            new Thread(() => handleConnect(context)).Start();
            localLogger.LogInformation("Connection handed off to thread.");
        }

        // ReSharper disable once FunctionNeverReturns
    }
}
