using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;

namespace CS3500.Networking;

/// <summary>
///   Represents a server task that waits for connections on a given
///   port and calls the provided delegate when a connection is made.
/// </summary>
public static class ServerConnection
{
    /// <summary>
    ///   Use on a TcpListener to handle new connections. Alert the calling program/function
    ///   via the handleConnect delegate.
    /// </summary>
    /// <param name="handleConnect">
    ///   Handler for what the user wants to do when a connection is made.
    ///   This should be run asynchronously via a new thread.
    /// </param>
    /// <param name="port"> The port (e.g., 11000) to listen on. </param>
    /// <param name="logger"> The logger instance used for logging connection events. </param>
    public static void WaitForConnections( Action<NetworkConnection> handleConnect, int port, ILogger logger )
    {
        TcpListener listener = new( IPAddress.Any, port );
        listener.Start();
        ILogger localLogger = logger;

        while (true)
        {
            localLogger.LogInformation( "Waiting for connections..." );
            NetworkConnection client = new( listener.AcceptTcpClient(), localLogger );
            localLogger.LogInformation( "Connection found, attempting to connect." );
            new Thread( () => handleConnect( client ) ).Start();
            localLogger.LogInformation( "Connection handed off to thread." );
        }
    }
}
