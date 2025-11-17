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
                    lock (world)
                    {
                        world.UpdateElement(message);
                    }
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
    /// <param name="s">Snake to draw.</param>
    public static async Task Draw(this Canvas2DContext context, Snake s)
    {
        float oldLineWidth = context.LineWidth;
        await context.SetLineWidthAsync(10);
        await context.BeginPathAsync();
        await context.MoveToAsync(s.Tail.X, s.Tail.Y);
        for (int i = 1; i < s.Body.Count - 1; i++)
        {
            await context.LineToAsync(s.Body[i].X, s.Body[i].Y);
        }
        await context.LineToAsync(s.Head.X, s.Head.Y);
        await context.SetStrokeStyleAsync("green");
        await context.StrokeAsync();
        await context.SetLineWidthAsync(oldLineWidth);
    }
}
