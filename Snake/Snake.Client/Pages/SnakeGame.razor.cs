using System.Diagnostics;
using System.Text.Json;
using CS3500.Networking;
using System.Threading.Tasks;
using Blazor.Extensions.Canvas.Canvas2D;

namespace CS3500.Snake.Client.Pages.SnakeGame;

using CS3500.Snake.Models;

public partial class SnakeGame
{
    /// <summary>
    /// The full game state that is sent by the server.
    /// </summary>
    private static World worldModel = null!;

    /// <summary>
    /// The unique ID assigned by the server for this player.
    /// </summary>
    private static int playerId = -1;

    /// <summary>
    /// Counter used for FPS computation.
    /// </summary>
    private static int frameNumber = 0;

    /// <summary>
    /// Stopwatch use the measure the time since rendering began.
    /// Helps compute the AvgFPs
    /// </summary>
    private static Stopwatch gameTimer = new();


    static SnakeGame()
    {
        gameTimer.Start();
    }

    private float AvgFps => frameNumber / (float)gameTimer.Elapsed.TotalSeconds;

    /// <summary>
    ///     Disconnect the network object from the server.
    /// </summary>
    private void DisconnectFromServer()
    {
        Logger.LogInformation("Disconnecting from server.");
        network.Disconnect();
        network = new NetworkConnection( Logger );
    }

    /// <summary>
    ///     <para>
    ///         Handler for the connect button.
    ///     </para>
    ///     <para>
    ///         Connect to the server, then continuously wait for messages and display them.
    ///     </para>
    /// </summary>
    private async void Connect()
    {
        await Task.Run(() =>
        {
            spinner = "spinner";
            InvokeAsync(StateHasChanged); // update UI to show connection status

            //TODO prevent commands before walls
            //TODO prevent client from sending more than one command per frame.

            network.Connect(serverNameOrAddress, serverPort);
            spinner = string.Empty;
            InvokeAsync(StateHasChanged);
            // Send first message: the username
            Logger.LogInformation($"Connected to server, sending username: {userName}");
            network.SendLine(userName);
            playerId = int.Parse(network.ReceiveLine());
            worldModel = new(int.Parse(network.ReceiveLine()));
            while (network.IsConnected)
            {
                string line = network.ReceiveLine();

                if (!string.IsNullOrWhiteSpace(line))
                {
                    lock (worldModel)
                    {
                        // Console.WriteLine(line);
                        worldModel.UpdateElement(line);
                    }
                }
            }
        });
        frameNumber = 0;
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
    /// <returns>????</returns>
    public static async Task Draw(this Canvas2DContext context, Snake s)
    {
        await context.BeginPathAsync();
        await context.MoveToAsync(s.Head.X, s.Head.Y);
        for (int i = 1; i < s.Body.Count - 1; i++)
        {
            await context.MoveToAsync(s.Body[i].X, s.Body[i].Y);
        }
        await.SetstrokeWidth( 10)
        await context.LineToAsync(s.Tail.X, s.Tail.Y);
        await context.StrokeAsync();
    }
}
