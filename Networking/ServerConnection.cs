// <copyright file="ServerConnection.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System.Net;
using Microsoft.Extensions.Logging;

namespace CS3500.Networking;

/// <summary>
///     Provides the functionality for running an HTTP server. Waits for incoming HTTP connections and 
///     hands them off to a user provided handler.
/// </summary>
public static class ServerConnection
{
    /// <summary>
    /// Listens for incoming HTTP requests on the specified address and port.
    /// Each request is given a new thread.
    /// </summary>
    /// <param name="handleConnect">
    ///     Handler for what the user wants to do when a connection is made.
    ///     This should be run asynchronously via a new thread.
    /// </param>
    /// <param name="address" >The URL address the web server is listening/hosting at.</param>
    /// <param name="port"> The port (e.g., 80, 8080, 11000) to listen on. </param>
    /// <param name="logger"> The logger instance used for logging connection events. </param>
    public static void WaitForConnections(Action<HttpListenerContext> handleConnect, string address, int port, ILogger logger)
    {
        HttpListener listener = new();
        listener.Prefixes.Add(address + port + "/");
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
