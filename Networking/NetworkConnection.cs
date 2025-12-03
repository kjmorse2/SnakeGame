// <copyright file="NetworkConnection.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System.Net.Sockets;
using System.Text;
using Microsoft.Extensions.Logging;

namespace CS3500.Networking;

/// <summary>
///     <para>
///         Wraps the StreamReader/Writer/TcpClient together so we
///         don't have to keep creating all three for network actions.
///     </para>
///     <para>
///         Note: In C#, the sealed keyword prevents further inheritance,
///         i.e., no class can derive from this one.  We do this because the
///         class is a stable, final abstraction around a TCP socket.
///     </para>
///     <para>
///         Implements IDisposable because we want to make sure that any given
///         network connection is "cleaned up" when we are done with it.
///     </para>
/// </summary>
public sealed class NetworkConnection : IDisposable
{
    /// <summary>
    ///     The logger used to record important events.
    /// </summary>
    private readonly ILogger networkLogger;

    /// <summary>
    ///     The connection/socket abstraction.
    /// </summary>
    private readonly TcpClient networkTcpClient;

    /// <summary>
    ///     Reading end of the connection.
    /// </summary>
    private StreamReader networkReader = null!;

    /// <summary>
    ///     Writing end of the connection.
    /// </summary>
    private StreamWriter? networkWriter;

    /// <summary>
    ///     Initializes a new instance of the <see cref="NetworkConnection" /> class.
    ///     <para>
    ///         Create a network connection object.
    ///     </para>
    /// </summary>
    /// <param name="tcpClient">
    ///     An already existing TcpClient.
    /// </param>
    /// <param name="logger"> The logging element. </param>
    public NetworkConnection(TcpClient tcpClient, ILogger logger)
    {
        networkTcpClient = tcpClient;
        networkLogger = logger;
        networkLogger.LogTrace("NetworkConnection created with provided tcpClient.");
        if (IsConnected)
        {
            networkLogger.LogDebug("NetworkConnection object connected to server");
            networkReader = new StreamReader(networkTcpClient.GetStream(), Encoding.UTF8);

            // AutoFlush ensures data is sent immediately
            networkWriter = new StreamWriter(networkTcpClient.GetStream(),  new UTF8Encoding(false)) { AutoFlush = true };
        }
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="NetworkConnection" /> class.
    ///     <para>
    ///         Create a network connection object.  The tcpClient will be unconnected at the start.
    ///     </para>
    /// </summary>
    /// <param name="logger">The logging element.</param>
    public NetworkConnection(ILogger logger)
        : this(new TcpClient(), logger)
    {
        networkLogger.LogTrace(
            "NetworkConnection with no connection created, connection is needed before communication.");
    }

    /// <summary>
    ///     Gets a value indicating whether the socket is connected.
    /// </summary>
    public bool IsConnected
    {
        get
        {
            networkLogger.LogTrace("Network Connection is checking connection status");
            return networkTcpClient.Connected;
        }
    }

    /// <summary>
    ///     Try to connect to the given host:port.
    /// </summary>
    /// <param name="host"> The URL or IP address, e.g., www.cs.utah.edu, or  127.0.0.1. </param>
    /// <param name="port"> The port, e.g., 11000. </param>
    public void Connect(string host, int port)
    {
        networkLogger.LogDebug($"NetworkConnection is attempting to connect to {host} on port {port}");
        networkTcpClient.Connect(host, port);
        networkReader = new StreamReader(networkTcpClient.GetStream(), Encoding.UTF8);

        // AutoFlush ensures data is sent immediately
        networkWriter = new StreamWriter(networkTcpClient.GetStream(), new UTF8Encoding(false)) { AutoFlush = true };
    }

    /// <summary>
    ///     If connected, disconnect the connection and clean
    ///     up (dispose) any streams.
    ///     <para>
    ///     </para>
    ///     <list type="number">
    ///         <item>
    ///             Then call the tcpclient object's Client.Shutdown method with SocketShutdown.Both.
    ///         </item>
    ///         <item>
    ///             Then dispose the writer and reader.
    ///         </item>
    ///         <item>
    ///             Finally close the tcpclient object.
    ///         </item>
    ///     </list>
    /// </summary>
    public void Disconnect()
    {
        if (IsConnected)
        {
            networkTcpClient.Client.Shutdown(SocketShutdown.Both);
            networkWriter!.Close();
            networkReader.Close();
            networkTcpClient.Close();
            networkLogger.LogDebug("NetworkConnection disconnected from current TcpClient.");
        }
    }

    /// <summary>
    ///     Automatically called with a using statement (see IDisposable).
    /// </summary>
    public void Dispose()
    {
        networkLogger.LogDebug("NetworkConnection was disposed of.");
        Disconnect();
    }

    /// <summary>
    ///     Read a message from the other side of the socket.  The message will contain
    ///     all characters up to the first new line. See <see cref="SendLineAsync" />.
    /// </summary>
    /// <remarks>
    ///     <list type="bullet">
    ///         <item>
    ///             It is possible for this method to block indefinitely if the other side
    ///             doesn't send any data.
    ///         </item>
    ///         <item>
    ///             It is possible for this method to return an empty string if the other side
    ///             sends an "empty" message.
    ///         </item>
    ///     </list>
    /// </remarks>
    /// <returns> The contents of the message. </returns>
    /// <exception cref="InvalidOperationException">
    ///     An InvalidOperationException will be thrown if the connection is not established.
    /// </exception>
    /// <exception cref="IOException">
    ///     Thrown if an I/O error occurs while reading from the stream, for example:
    ///     <list type="bullet">
    ///         <item>The stream was closed (usually by the other side quitting).</item>
    ///         <item>The underlying network connection was lost.</item>
    ///     </list>
    ///     <remarks>
    ///         It is acceptable (in most cases) for your external code to catch the generic
    ///         (base type) "Exception" type when using this method, as regardless of which exception
    ///         is thrown, the connection is no longer usable.
    ///     </remarks>
    /// </exception>
    public string ReceiveLine()
    {
        networkLogger.LogDebug("Network connection is attempting to receive message.");
        try
        {
            string? received = networkReader.ReadLine();

            if (!IsConnected || received is null)
            {
                networkLogger.LogDebug("NetworkConnection did not receive message, connection was closed");
                throw new InvalidOperationException("Connection was closed");
            }

            networkLogger.LogDebug("NetworkConnection received message: " + received);

            return received;
        }
        catch (Exception e)
        {
            networkLogger.LogDebug("Message not received, other error occurred: " + e.Message);
            throw new IOException("Error getting message: " + e.Message);
        }
    }

    /// <summary>
    ///     Send a message to the remote server.  If the <paramref name="message" /> contains
    ///     new lines, these will be treated on the receiving side as multiple messages.
    ///     This method should attach a newline to the end of the <paramref name="message" />
    ///     (by using WriteLine).
    ///     If this operation can not be completed (e.g. because this NetworkConnection is not
    ///     connected), throw an InvalidOperationException.
    /// </summary>
    /// <param name="message"> The string of characters to send. </param>
    public void SendLine(string message)
    {
        networkLogger.LogDebug($"NetworkConnection is attempting to send message: {message}");
        try
        {
            networkWriter!.WriteLine(message.Trim());
            networkLogger.LogDebug($"NetworkConnection sent message: {message}");
        }
        catch (Exception e)
        {
            networkLogger.LogDebug("Message not sent: " + e.Message);
            throw new InvalidOperationException("Error sending message: " + e.Message);
        }
    }
}
