// <copyright file="ChatServer.cs" company="UofU-CS3500">
// Copyright (c) 2024 UofU-CS3500. All rights reserved.
// </copyright>

using CS3500.Networking;
using Microsoft.Extensions.Logging;

namespace CS3500.Chatting;

/// <summary>
///   A simple ChatServer that handles clients separately and replies with a static message.
/// </summary>
public partial class ChatServer
{
    /// <summary>
    ///   The main program.
    /// </summary>
    /// <param name="args"> ignored. </param>
    /// <returns> A Task. Not really used. </returns>
    private static void Main()
    {
        // TODO - build a logging system and pass a logger to WaitForConnections

        ServerConnection.WaitForConnections( HandleConnect, 11_000, null );
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
    ///     All actions on the list of connected clients must be thread-safe!
    ///   </para>
    ///   <para> 
    ///     All important events (connections, disconnections, messages received, messages sent, errors, etc.)
    ///     must be logged using the logging system at the appropriate log level.
    ///   </para>
    /// </summary>
    private static void HandleConnect( NetworkConnection connection )
    {
        // handle all messages until disconnect.
        try
        {
            while ( true )
            {
                var message = connection.ReceiveLine( );

                connection.SendLine( $"thanks, I got {message}!" );
            }
        }
        catch ( Exception )
        {
            // TODO: do anything necessary to handle a disconnected client in here,
        }
    }
}