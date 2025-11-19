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
public partial class SnakeGame
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
    private void DisconnectFromServer()
    {
        Logger.LogInformation("Disconnecting from server.");
        // TODO: Consider catching and handling disconnect exceptions gracefully.
        connection.Disconnect();
        _jsModule.InvokeVoidAsync("ToggleAnimation", false);
        connection = new NetworkConnection(Logger);
        Logger.LogInformation("Disconnected and reset connection instance.");
    }

    /// <summary>
    /// Connects to the server, negotiates identity, initializes the world, and begins receiving updates.
    /// The receive loop runs on a background thread. The render loop is driven from JS via requestAnimationFrame.
    /// </summary>
    private async void Connect()
    {
        Logger.LogInformation("Attempting to connect to server {Host}:{Port} as '{Player}'.", serverHost, serverPort, playerName);
        await Task.Run(() =>
        {
            try
            {
                // Show spinner while establishing connection
                connectionSpinnerClass = "spinner";
                InvokeAsync(StateHasChanged); // update UI to show connection status

                // Connect to the server. This can throw on network errors.
                // TODO: Catch and handle connection exceptions (e.g., server not reachable, refused, DNS issues) and update UI.
                connection.Connect(serverHost, serverPort);
                Logger.LogInformation("Connected to server.");

                connectionSpinnerClass = string.Empty;
                InvokeAsync(StateHasChanged);

                // Send first message: the username so the server can label us.
                Logger.LogInformation("Sending username to server.");
                // TODO: Consider catching send exceptions gracefully.
                connection.SendLine(playerName);

                // Server replies with our player id and world size.
                // TODO: Consider validating and catching parse exceptions for server responses.
                PlayerId = int.Parse(connection.ReceiveLine());
                World = new World(int.Parse(connection.ReceiveLine()));
                Logger.LogInformation("Received player id {PlayerId} and world size {WorldSize}.", PlayerId, World.Size);

                // Start JS-driven animation loop now that the world exists.
                // TODO: Consider catching JS interop errors and handling gracefully.
                Logger.LogInformation("Animation loop started via JS interop.");

                // TODO: Make more dry? possible to do this without 2 loops? 
                // Receive world updates.
                while (connection.IsConnected)
                {
                    string message = connection.ReceiveLine();
                    if (!string.IsNullOrWhiteSpace(message))
                    {
                        // Update the world with the server-provided JSON payload
                        if (World.UpdateElement(message, PlayerId))
                        {
                            break;
                        }
                    }
                }

                GameTimer.Restart();
                FrameCount = 0;
                _jsModule.InvokeVoidAsync("ToggleAnimation", true);

                // Receive world updates.
                while (connection.IsConnected)
                {
                    // TODO: Consider handling network read exceptions and malformed messages gracefully.
                    string message = connection.ReceiveLine();
                    if (!string.IsNullOrWhiteSpace(message))
                    {
                        // Update the world with the server-provided JSON payload
                        World.UpdateElement(message);
                    }
                }
                Logger.LogInformation("Connection closed by server or client.");
            }
            catch(Exception e)
            {
                Logger.LogInformation("Error occurred while updating your world: " + e.Message);
                DisconnectFromServer();
            }
        });

        Logger.LogInformation("FPS metrics reset after connection end.");
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
            if (powerUp.IsDead)
            {
                // TODO: Consider removing dead items earlier to avoid per-frame checks.
                continue;
            }
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
