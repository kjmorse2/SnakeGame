using System.Diagnostics;
using CS3500.Networking;
using Blazor.Extensions.Canvas.Canvas2D;
using Microsoft.JSInterop;

namespace CS3500.Snake.Client.Pages.SnakeGame;

using CS3500.Snake.Models;

/// <summary>
/// Code-behind for the SnakeGame Razor component. Manages the network connection,
/// world state, animation timing metrics, and JS interop entry points used by the view.
/// </summary>
public partial class SnakeGame : IDisposable
{
    /// <summary>
    /// The authoritative world model as provided by the server.
    /// </summary>
    private static World World = null!;

    /// <summary>
    /// The unique ID assigned by the server for this player.
    /// </summary>
    private static int PlayerId = -1;

    /// <summary>
    /// Frame counter used for FPS computation.
    /// </summary>
    private static int FrameCount;

    /// <summary>
    /// Wall-clock timer used to compute average frames-per-second.
    /// </summary>
    private static readonly Stopwatch GameTimer = new();

    /// <summary>
    /// Cancellation token source used to stop background network receive loops on user disconnect.
    /// </summary>
    private CancellationTokenSource? _receiveCts;

    /// <summary>
    /// Static ctor to begin timing immediately. FPS resets when reconnecting.
    /// </summary>
    static SnakeGame()
    {
        GameTimer.Start();
    }

    /// <summary>
    /// Average frames per second since <see cref="GameTimer"/> was started or last restarted.
    /// Used only for HUD display.
    /// </summary>
    private float AvgFps => FrameCount / (float)GameTimer.Elapsed.TotalSeconds;

    /// <summary>
    /// Disconnects from the server and resets the <see cref="connection"/> instance.
    /// Safe to call when not connected.
    /// </summary>
    private async Task DisconnectFromServer()
    {
        // Signal any background receive loop to stop before touching the socket
        _receiveCts?.Cancel();
        _receiveCts?.Dispose();
        _receiveCts = null;

        await _jsModule.InvokeVoidAsync("ToggleAnimation", false);
        Logger.LogInformation("Disconnecting from server.");
        try
        {
            connection.Disconnect();
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

    private Task DisconnectFromServer(string errorMessage)
    {
        Logger.LogInformation("Disconnecting from server due to error: " + errorMessage);
        return DisconnectFromServer();
    }

    /// <summary>
    /// Connects to the server, negotiates identity, initializes the world, and begins receiving updates.
    /// The receive loop runs on a background thread. The render loop is driven from JS via requestAnimationFrame.
    /// </summary>
    private async void Connect()
    {
        // Ensure any previous receive task is canceled before starting a new one
        _receiveCts?.Cancel();
        _receiveCts?.Dispose();
        _receiveCts = new CancellationTokenSource();
        var token = _receiveCts.Token;

        Logger.LogInformation("Attempting to connect to server {Host}:{Port} as '{Player}'.", serverHost, serverPort, playerName);
        await Task.Run(() =>
        {
            try
            {
                // Show spinner while establishing connection
                connectionSpinnerClass = "spinner";
                InvokeAsync(StateHasChanged); // update UI to show connection status

                // Connect to the server. This can throw on network errors.
                connection.Connect(serverHost, serverPort);
                Logger.LogInformation("Connected to server.");

                connectionSpinnerClass = string.Empty;
                InvokeAsync(StateHasChanged);

                // Send first message: the username so the server can label us.
                Logger.LogInformation("Sending username to server.");
                connection.SendLine(playerName);

                // Server replies with our player id and world size.
                PlayerId = int.Parse(connection.ReceiveLine());
                World = new World(int.Parse(connection.ReceiveLine()));
                Logger.LogInformation("Received player id {PlayerId} and world size {WorldSize}.", PlayerId, World.Size);

                // Start JS-driven animation loop now that the world exists.

                // First reception loop, runs until our snake appears in the world.
                while (!token.IsCancellationRequested
                       && connection.IsConnected
                       && !World.Snakes.TryGetValue(PlayerId, out Snake? _ ))
                {
                    string message = connection.ReceiveLine();
                    World.UpdateElement(message);
                }

                // Now that our snake exists, start the animation loop
                if (!token.IsCancellationRequested)
                {
                    _jsModule.InvokeVoidAsync("ToggleAnimation", token, true);
                    Logger.LogInformation("Animation loop started via JS interop.");
                }

                // Start tracking FPS from this point forward
                GameTimer.Restart();
                FrameCount = 0;

                // Receive world updates while drawing.
                while (!token.IsCancellationRequested && connection.IsConnected)
                {
                    string message = connection.ReceiveLine();
                    // Update the world with the server-provided JSON payload
                    World.UpdateElement(message);
                }
                Logger.LogInformation("Connection closed by server or client.");
            }
            catch(Exception e)
            {
                // If cancellation was requested, treat any exceptions from ReceiveLine/IO as expected during shutdown
                if (token.IsCancellationRequested)
                {
                    Logger.LogInformation("Receive loop canceled by user disconnect");
                }
                else
                {
                    Console.WriteLine(e.Message);
                    DisconnectFromServer(e.Message);
                }
            }
        }, token);

        // Cleanup CTS after background task completes
        _receiveCts?.Dispose();
        _receiveCts = null;

        Logger.LogInformation("FPS metrics reset after connection end.");
    }

    public void Dispose()
    {
        context.Dispose();
        if (_jsModule is IDisposable jsModuleDisposable)
        {
            jsModuleDisposable.Dispose();
        }
        else
        {
            _ = _jsModule.DisposeAsync().AsTask();
        }

        connection.Dispose();
        _receiveCts?.Dispose();
    }
}

/// <summary>
/// Canvas drawing helper extensions for rendering world elements using a <see cref="Canvas2DContext"/>.
/// </summary>
public static class ContextExtensions
{
    /// <summary>
    /// Array of colors to for the snake to be drawn in .
    /// </summary>
    private static readonly string[ ] SnakeColors =
     {
      "lime", "cyan", "yellow", "orange", "magenta", "red", "blue", "white",
     };
    /// <summary>
    /// Draws a single snake as a stroked polyline from tail to head.
    /// </summary>
    /// <param name="context">The canvas 2D context to draw with.</param>
    /// <param name="snake">The snake to render.</param>

    public static async Task Draw(this Canvas2DContext context, Snake snake)
    {
        // Temporarily set stroke thickness for snake geometry
        float oldLineWidth = context.LineWidth;
        await context.SetLineWidthAsync(10);
        await context.SetLineCapAsync(LineCap.Round);
        await context.SetLineJoinAsync(LineJoin.Round);

        await context.BeginPathAsync();
        await context.MoveToAsync(snake.Tail.X, snake.Tail.Y);
        for (int i = 1; i < snake.Body.Count - 1; i++)
        {
            await context.LineToAsync(snake.Body[i].X, snake.Body[i].Y);
        }
        await context.LineToAsync(snake.Head.X, snake.Head.Y);

        await context.SetStrokeStyleAsync(SnakeColors[ snake.Id % SnakeColors.Length ]);
        await context.StrokeAsync();

        await context.SetFontAsync("14px Arial");
        await context.SetFillStyleAsync("white");
        await context.FillTextAsync($" {snake.Name}:{snake.Score}", snake.Head.X - 20, snake.Head.Y + 30);

        // Restore previous stroke thickness
        await context.SetLineWidthAsync(oldLineWidth);
    }

    /// <summary>
    /// Draws a collection of snakes by delegating to <see cref="Draw(Canvas2DContext, Snake)"/>.
    /// </summary>
    /// <param name="context">The canvas 2D context to draw with.</param>
    /// <param name="snakes">The sequence of snakes to render.</param>
    public static async Task Draw(this Canvas2DContext context, IEnumerable<Snake> snakes)
    {
        foreach (Snake snake in snakes)
        {
            await context.Draw(snake);
        }
    }

    /// <summary>
    /// Draws a single power-up as a filled square centered on its position.
    /// </summary>
    /// <param name="context">The canvas 2D context to draw with.</param>
    /// <param name="powerUp">The power-up to render.</param>
    public static async Task Draw(this Canvas2DContext context, PowerUp powerUp)
    {
        await context.FillRectAsync(powerUp.Position.X - 8, powerUp.Position.Y - 8, 16, 16);
    }

    /// <summary>
    /// Draws a collection of power-ups, skipping those marked dead.
    /// </summary>
    /// <param name="context">The canvas 2D context to draw with.</param>
    /// <param name="powerUps">The sequence of power-ups to render.</param>
    public static async Task Draw(this Canvas2DContext context, IEnumerable<PowerUp> powerUps)
    {
        foreach (PowerUp powerUp in powerUps)
        {
            await context.SetFillStyleAsync("yellow");
            await context.Draw(powerUp);
        }
    }

    /// <summary>
    /// Draws all walls using filled segments.
    /// </summary>
    /// <param name="context">The canvas 2D context to draw with.</param>
    /// <param name="walls">The sequence of walls to render.</param>
    public static async Task Draw(this Canvas2DContext context, IEnumerable<Wall> walls)
    {
        await context.SetFillStyleAsync("red");
        foreach (Wall wall in walls)
        {
            await context.Draw(wall);
        }
    }

    /// <summary>
    /// Draws a single wall using its discrete segments.
    /// </summary>
    /// <param name="context">The canvas 2D context to draw with.</param>
    /// <param name="wall">The wall to render.</param>
    public static async Task Draw(this Canvas2DContext context, Wall wall)
    {
        foreach (Point2D segment in wall.GetSegments())
        {
            await context.FillRectAsync(segment.X - 25, segment.Y - 25, Wall.SegmentSize, Wall.SegmentSize);
        }
    }
}
