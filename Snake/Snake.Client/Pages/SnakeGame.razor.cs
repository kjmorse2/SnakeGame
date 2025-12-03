// <copyright file="SnakeGame.razor.cs" company="U of U CS3500">
// Copyright (c) U of U CS3500, Kenneth Morse, and Hunter Simmons. All rights reserved.
// </copyright>

using System.Diagnostics;
using CS3500.Networking;
using CS3500.Snake.Models;
using Microsoft.Data.SqlClient;
using Microsoft.JSInterop;

namespace CS3500.Snake.Client.Pages.SnakeGame;

/// <summary>
///     Code-behind for the SnakeGame Razor component. Manages the network connection,
///     world state, animation timing metrics, and JS interop entry points used by the view.
/// </summary>
public partial class SnakeGame : IDisposable
{
    /// <summary>
    ///     Wall-clock timer used to compute average frames-per-second.
    /// </summary>
    private static readonly Stopwatch GameTimer = new();

    /// <summary>
    ///     Frame counter used for FPS computation.
    /// </summary>
    private static int FrameCount;

    /// <summary>
    ///     The unique ID assigned by the server for this player.
    /// </summary>
    private static int PlayerId = -1;

    /// <summary>
    ///     The authoritative world model as provided by the server.
    /// </summary>
    private static World World = null!;

    /// <summary>
    ///     Cancellation token source used to stop background network receive loops on user disconnect.
    /// </summary>
    private CancellationTokenSource? receiveCts;

    /// <summary>
    /// A database interface for the Snake game.
    /// </summary>
    private DatabaseInterface dbInterface = new DatabaseInterface();

    /// <summary>
    ///     Initializes static members of the <see cref="SnakeGame" /> class.
    /// </summary>
    static SnakeGame()
    {
        GameTimer.Start();
    }

    /// <summary>
    ///     Gets average frames per second since <see cref="GameTimer" /> was started or last restarted.
    ///     Used only for HUD display.
    /// </summary>
    private float AvgFps => FrameCount / (float)GameTimer.Elapsed.TotalSeconds;

    /// <summary>
    ///     Releases all unmanaged and manage resources held by the SnakeGame component.
    /// </summary>
    public void Dispose()
    {
        // Necessary because context could technically not exist before Dispose is called.
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (context != null)
        {
            context.Dispose();
        }

        if (jsModule is IDisposable jsSync)
        {
            jsSync.Dispose();
        }
        else if (jsModule is IAsyncDisposable jsAsync)
        {
            jsAsync.DisposeAsync();
        }

        connection.Dispose();
        receiveCts?.Dispose();
    }

    /// <summary>
    ///     Connects to the server, negotiates identity, initializes the world, and begins receiving updates.
    ///     The receive loop runs on a background thread. The render loop is driven from JS via requestAnimationFrame.
    /// </summary>

    // ReSharper disable once UnusedMember.Local
    private async void Connect()
    {
        // Ensure any previous receive task is canceled before starting a new one
        receiveCts?.Cancel();
        receiveCts?.Dispose();
        receiveCts = new CancellationTokenSource();
        CancellationToken token = receiveCts.Token;

        string popupMessage = string.Empty;
        Logger.LogInformation(
            "Attempting to connect to server {Host}:{Port} as '{Player}'.",
            serverHost,
            serverPort,
            playerName);
        await Task.Run(
            () =>
            {
                try
                {
                    // Show spinner while establishing connection
                    connectionSpinnerClass = "spinner";
                    InvokeAsync(StateHasChanged); // update UI to show connection status

                    // Connect to the server. This can throw on network errors.
                    try
                    {
                        connection.Connect(serverHost, serverPort);
                    }
                    catch
                    {
                        popupMessage = "Could not connect to server, please check the host and port.";
                        throw;
                    }

                    Logger.LogInformation("Connected to server.");
                    dbInterface.NewGame();

                    connectionSpinnerClass = string.Empty;
                    InvokeAsync(StateHasChanged);

                    // Send first message: the username so the server can label us.
                    Logger.LogInformation("Sending username to server.");
                    try
                    {
                        connection.SendLine(playerName.Length > 16 ? playerName.Substring(0, 16) : playerName);
                    }
                    catch
                    {
                        popupMessage = "Could not send username to server.";
                        throw;
                    }

                    // Server replies with our player id and world size.
                    PlayerId = int.Parse(connection.ReceiveLine());
                    World = new World(int.Parse(connection.ReceiveLine()));
                    Logger.LogInformation(
                        "Received player id {PlayerId} and world size {WorldSize}.",
                        PlayerId,
                        World.Size);

                    // Start JS-driven animation loop now that the world exists.

                    // First reception loop, runs until our snake appears in the world.
                    try
                    {
                        while (!token.IsCancellationRequested
                               && connection.IsConnected
                               && !World.Snakes.TryGetValue(PlayerId, out Models.Snake? _))
                        {
                            string message = connection.ReceiveLine();
                            World.UpdateElement(message);
                        }
                    }
                    catch
                    {
                        popupMessage = "Connection lost while waiting for game to start.";
                        throw;
                    }

                    World.WallsLoaded = true;
                    jsModule.InvokeVoidAsync("StartControl", token, true);

                    // Now that our snake exists, start the animation loop
                    if (!token.IsCancellationRequested)
                    {
                        jsModule.InvokeVoidAsync("ToggleAnimation", token, true);
                        Logger.LogInformation("Animation loop started via JS interop.");
                    }

                    // Start tracking FPS from this point forward
                    GameTimer.Restart();
                    FrameCount = 0;

                    try
                    {
                        // Receive world updates while drawing.
                        while (!token.IsCancellationRequested && connection.IsConnected)
                        {
                            string message = connection.ReceiveLine();

                            // Update the world with the server-provided JSON payload
                            World.UpdateElement(message);
                        }

                        Logger.LogInformation("Connection closed by server or client.");
                    }
                    catch
                    {
                        popupMessage = "Connection lost during game.";
                        throw;
                    }
                }
                catch (Exception e)
                {
                    // If cancellation was requested, treat any exceptions from ReceiveLine/IO as expected during shutdown
                    if (token.IsCancellationRequested)
                    {
                        Logger.LogInformation("Receive loop canceled by user disconnect");
                        DisconnectFromServer();
                    }
                    else
                    {
                        Console.WriteLine(e.Message);
                        AlertPopup(popupMessage);
                        DisconnectFromServer(e.Message);
                    }
                }
            },
            token);

        // Cleanup CTS after background task completes
        receiveCts?.Dispose();
        receiveCts = null;

        Logger.LogInformation("FPS metrics reset after connection end.");
    }

    /// <summary>
    ///     Safely disconnects the client from the Snake server and resets the state.
    /// </summary>
    private void DisconnectFromServer()
    {
        _ = DisconnectFromServerAsync();
    }

    /// <summary>
    ///     Disconnects from the server with an error message.
    /// </summary>
    /// <param name="errorMessage">The error message to log.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private Task DisconnectFromServer(string errorMessage)
    {
        Logger.LogInformation("Disconnecting from server due to error: " + errorMessage);
        return DisconnectFromServerAsync();
    }

    /// <summary>
    ///     Disconnects from the server and resets the <see cref="connection" /> instance.
    ///     Safe to call when not connected.
    /// </summary>
    private async Task DisconnectFromServerAsync()
    {
        // Signal any background receive loop to stop before touching the socket
        receiveCts?.Cancel();
        receiveCts?.Dispose();
        receiveCts = null;

        await jsModule.InvokeVoidAsync("ToggleAnimation", false);
        Logger.LogInformation("Disconnecting from server.");
        try
        {
            connection.Disconnect();
            dbInterface.EndGame();
        }
        catch (SqlException e)
        {
            Logger.LogError("SQL Error during disconnect: " + e.Message);
        }
        catch (Exception e)
        {
            Logger.LogError("Error during disconnect: " + e.Message);
        }

        connection = new NetworkConnection(Logger);
        Logger.LogInformation("Disconnected and reset connection instance.");
        await DisconnectScreenAsync();
        World.Clear();
    }
}
