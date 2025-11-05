// <copyright file="NetworkConnection.cs" company="UofU-CS3500">
// Copyright (c) 2024 UofU-CS3500. All rights reserved.
// </copyright>

using System.Net.Sockets;
using System.Text;
using Microsoft.Extensions.Logging;
namespace CS3500.Networking;

/// <summary>
///   <para>
///     Wraps the StreamReader/Writer/TcpClient together so we
///     don't have to keep creating all three for network actions.
///   </para>
///   <para>
///     Note: In C#, the sealed keyword prevents further inheritance,
///     i.e., no class can derive from this one.  We do this because the
///     class is a stable, final abstraction around a TCP socket.
///   </para>
///   <para>
///     Implements IDisposable because we want to make sure that any given
///     network connection is "cleaned up" when we are done with it. 
///   </para>
/// </summary>
public sealed class NetworkConnection : IDisposable
{
    /// <summary>
    ///   The connection/socket abstraction
    /// </summary>
    private TcpClient _tcpClient = new();

    /// <summary>
    ///   Reading end of the connection
    /// </summary>
    private StreamReader _reader = StreamReader.Null;

    /// <summary>
    ///   Writing end of the connection
    /// </summary>
    private StreamWriter? _writer = StreamWriter.Null;

    /// <summary>
    ///   Initializes a new instance of the <see cref="NetworkConnection"/> class.
    ///   <para>
    ///     Create a network connection object.
    ///   </para>
    /// </summary>
    /// <param name="tcpClient">
    ///   An already existing TcpClient
    /// </param>
    /// <param name="logger"> The logging element. </param>
    public NetworkConnection( TcpClient tcpClient, ILogger logger )
    {
        // FIXME: save the logger
        _tcpClient = tcpClient;
        if ( IsConnected )
        {
            // Only establish the reader/writer if the provided TcpClient is already connected.
            _reader = new StreamReader( _tcpClient.GetStream(), Encoding.UTF8 );
            _writer = new StreamWriter( _tcpClient.GetStream(), Encoding.UTF8 ) { AutoFlush = true }; // AutoFlush ensures data is sent immediately
        }
    }

    /// <summary>
    ///   Initializes a new instance of the <see cref="NetworkConnection"/> class.
    ///   <para>
    ///     Create a network connection object.  The tcpClient will be unconnected at the start.
    ///   </para>
    /// </summary>
    public NetworkConnection( ILogger logger )
        : this( new TcpClient(), logger )
    {
    }

    /// <summary>
    /// Gets a value indicating whether the socket is connected.
    /// </summary>
    public bool IsConnected
    {
        get
        {
            return false;
            // TODO: implement this
            // Reminder: logging is required.
        }
    }


    /// <summary>
    ///   Try to connect to the given host:port. 
    /// </summary>
    /// <param name="host"> The URL or IP address, e.g., www.cs.utah.edu, or  127.0.0.1. </param>
    /// <param name="port"> The port, e.g., 11000. </param>
    public void Connect( string host, int port )
    {
        // TODO: implement this
        throw new NotImplementedException();
    }



    /// <summary>
    ///   Send a message to the remote server.  If the <paramref name="message"/> contains
    ///   new lines, these will be treated on the receiving side as multiple messages.
    ///   This method should attach a newline to the end of the <paramref name="message"/>
    ///   (by using WriteLine).
    ///   If this operation can not be completed (e.g. because this NetworkConnection is not
    ///   connected), throw an InvalidOperationException.
    /// </summary>
    /// <param name="message"> The string of characters to send. </param>
    public void SendLine( string message )
    {
        // TODO: Implement this
        throw new NotImplementedException();
    }


    /// <summary>
    ///   Read a message from the other side of the socket.  The message will contain
    ///   all characters up to the first new line. See <see cref="SendLineAsync"/>.
    /// </summary>
    /// <remarks>
    ///   <list type="bullet">
    ///     <item>
    ///       It is possible for this method to block indefinitely if the other side
    ///       doesn't send any data.
    ///     </item>
    ///     <item>
    ///       It is possible for this method to return an empty string if the other side
    ///       sends an "empty" message.
    ///     </item>
    ///   </list>
    /// </remarks>
    /// <returns> The contents of the message. </returns>
    /// <exception cref="InvalidOperationException">
    ///   An InvalidOperationException will be thrown if the connection is not established.
    /// </exception>
    /// <exception cref="IOException">
    ///   Thrown if an I/O error occurs while reading from the stream, for example:
    ///   <list type="bullet">
    ///     <item>The stream was closed (usually by the other side quitting).</item>
    ///     <item>The underlying network connection was lost.</item>
    ///   </list>
    ///   <remarks>
    ///     It is acceptable (in most cases) for your external code to catch the generic
    ///     (base type) "Exception" type when using this method, as regardless of which exception
    ///     is thrown, the connection is no longer usable.
    ///   </remarks>
    /// </exception>
    public string ReceiveLine( )
    {
        // TODO: implement this
        throw new NotImplementedException();

    }

    /// <summary>
    ///   If connected, disconnect the connection and clean 
    ///   up (dispose) any streams.
    ///   <para>
    ///     TODO:
    ///   </para>
    ///   <list type="number">
    ///     <item>
    ///       Then call the tcpclient object's client.Shutdown method with SocketShutdown.Both.
    ///     </item>
    ///     <item>
    ///       Then dispose the writer and reader.
    ///     </item>
    ///     <item>
    ///       Finally close the tcpclient object.
    ///     </item>
    ///   </list>
    /// </summary>
    public void Disconnect( )
    {
        //TODO: implement this
        throw new NotImplementedException();
    }

    /// <summary>
    ///   Automatically called with a using statement (see IDisposable)
    /// </summary>
    public void Dispose( )
    {
        Disconnect();
    }
}
