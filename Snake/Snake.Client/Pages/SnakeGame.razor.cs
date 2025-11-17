using System.Diagnostics;
using System.Text.Json;
using CS3500.Networking;
using System.Threading.Tasks;
using Blazor.Extensions.Canvas.Canvas2D;
using Microsoft.JSInterop;

namespace CS3500.Snake.Client.Pages.SnakeGame;

using CS3500.Snake.Models;

public partial class SnakeGame
{
    /// <summary>
    /// The full game state that is sent by the server.
    /// </summary>
    private static World world = null!;

    /// <summary>
    /// The unique ID assigned by the server for this player.
    /// </summary>
    private static int playerId = -1;

    /// <summary>
    /// Counter used for FPS computation.
    /// </summary>
    private static int frameCount = 0;

    /// <summary>
    /// Stopwatch used to measure the time since rendering began for FPS computation.
    /// </summary>
    private static readonly Stopwatch gameTimer = new();

    static SnakeGame()
    {
        gameTimer.Start();
    }

    private float AvgFps => frameCount / (float)gameTimer.Elapsed.TotalSeconds;

    /// <summary>
    ///     Disconnect the network object from the server.
    /// </summary>
    private void DisconnectFromServer()
    {
        Logger.LogInformation("Disconnecting from server.");
        connection.Disconnect();
        connection = new NetworkConnection(Logger);
    }

    /// <summary>
    ///     Connect to the server, then continuously wait for messages and update the world.
    /// </summary>
    private async void Connect()
    {
        await Task.Run(() =>
        {
            connectionSpinnerClass = "spinner";
            InvokeAsync(StateHasChanged); // update UI to show connection status

            //TODO prevent commands before walls
            //TODO prevent client from sending more than one command per frame.

            connection.Connect(serverHost, serverPort);
            connectionSpinnerClass = string.Empty;
            InvokeAsync(StateHasChanged);

            // Send first message: the username
            Logger.LogInformation($"Connected to server, sending username: {playerName}");
            connection.SendLine(playerName);

            // Server replies with our player id and world size
            playerId = int.Parse(connection.ReceiveLine());
            world = new World(int.Parse(connection.ReceiveLine()));

            // Begin JS animation loop after successful connection
            _jsModule.InvokeVoidAsync("ToggleAnimation", true);

            // Receive world updates
            while (connection.IsConnected)
            {
                string message = connection.ReceiveLine();
                if (!string.IsNullOrWhiteSpace(message))
                {
                    world.UpdateElement(message);
                }
            }
        });

        // Reset FPS metrics after connection loop
        frameCount = 0;
        gameTimer.Restart();
    }
}

/// <summary>
/// Canvas drawing helper class for drawing the snake.
/// </summary>
public static class ContextExtensions
{
    /// <summary>
    ///  Draws the full snake to the canvas.
    /// </summary>
    /// <param name="context">Canvas context</param>
    /// <param name="snake">Snake to draw.</param>
    public static async Task Draw(this Canvas2DContext context, Snake snake)
    {
        float oldLineWidth = context.LineWidth;
        await context.SetLineWidthAsync(10);
        await context.BeginPathAsync();
        await context.MoveToAsync(snake.Tail.X, snake.Tail.Y);
        for (int i = 1; i < snake.Body.Count - 1; i++)
        {
            await context.LineToAsync(snake.Body[i].X, snake.Body[i].Y);
        }
        await context.LineToAsync(snake.Head.X, snake.Head.Y);
        await context.SetStrokeStyleAsync("green");
        await context.StrokeAsync();
        await context.SetLineWidthAsync(oldLineWidth);
    }

    public static async Task Draw(this Canvas2DContext context, IEnumerable<Snake> snakes)
    {
        foreach (var snake in snakes)
        {
            await context.Draw(snake);
        }
    }

    public static async Task Draw(this Canvas2DContext context, PowerUp powerUp)
    {
        await context.FillRectAsync(powerUp.Position.X - 8, powerUp.Position.Y - 8, 16, 16);
    }
    public static async Task Draw(this Canvas2DContext context, IEnumerable<PowerUp> powerUps)
    {
        foreach (var powerUp in powerUps)
        {
            await context.SetFillStyleAsync("yellow");
            if (powerUp.IsDead)
            {
                // TODO make memory efficient.
                continue;
            }
            await context.Draw(powerUp);
        }
    }

    public static async Task Draw(this Canvas2DContext context, IEnumerable<Wall> walls)
    {
        await context.SetFillStyleAsync("red");
        foreach (var wall in walls)
        {
            await context.Draw(wall);
        }
    }

    public static async Task Draw(this Canvas2DContext context, Wall wall)
    {
        foreach (Point2D segment in wall.GetSegments())
        {
            await context.FillRectAsync(segment.X - 25, segment.Y - 25, Wall.SegmentSize, Wall.SegmentSize);
        }
    }
}
