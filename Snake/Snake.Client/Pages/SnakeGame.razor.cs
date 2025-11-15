using System.Diagnostics;
using System.Text.Json;
using CS3500.Networking;
using System.Threading.Tasks;
using Blazor.Extensions.Canvas.Canvas2D;

namespace CS3500.Snake.Client.Pages.SnakeGame;

using CS3500.Snake.Models;


public partial class SnakeGame
{
    private CancellationTokenSource CancelTokenSource = new();

    private static World worldModel = null!;

    private static int playerId = -1;

    private static int frameNumber = 0;

    private static Stopwatch gameTimer = new();

    private float AvgFps
    {
        get
        {
            return frameNumber / (float)gameTimer.Elapsed.TotalSeconds;
        }
    }

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

            // try
            // {
                // Communicating with the server needs to be asynchronous so that the UI thread
                // can continue drawing. Thus, we run on a background Task.
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
            // }
            // catch(Exception e)
            // {
            //     Logger.LogInformation(e.Message);
            //     noServerMessage = $"There is no server at: {serverNameOrAddress}, port: {serverPort}.";
            //     Logger.LogInformation(noServerMessage);
            //     spinner = string.Empty;
            //     InvokeAsync(StateHasChanged);
            //     // you can simulate this by trying to connect to a port where no server is running.
            // }
        });
    }

    private async Task RecieveLoop(CancellationToken ct)
    {
        while (network.IsConnected && !ct.IsCancellationRequested)
        {
            string line = await Task.Run(() => network.ReceiveLine());

            if (!string.IsNullOrWhiteSpace(line))
            {
                lock (worldModel)
                {
                    Console.WriteLine(line);
                    worldModel.UpdateElement(line);
                }
            }
        }
    }

    private async Task RenderLoop(CancellationToken ct)
    {
        frameNumber = 0;
        gameTimer.Restart();
        while (network.IsConnected && !ct.IsCancellationRequested)
        {
            await Task.Delay(16, ct);
            await InvokeAsync(async () =>
            {
                await Draw();
                frameNumber++;
            });
        }

    }
}
public static class ContextExtensions
{
    public static async Task Draw(this Canvas2DContext context, Snake s)
    {
        await context.BeginPathAsync();
        await context.MoveToAsync(s.Head.X, s.Head.Y);
        for (int i = 1; i < s.Body.Count - 1; i++)
        {
            await context.MoveToAsync(s.Body[i].X, s.Body[i].Y);
        }
        await context.LineToAsync(s.Tail.X, s.Tail.Y);
    }
}