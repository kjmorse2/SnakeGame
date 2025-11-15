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
    ///  Used to stop the receive and render loops when disconnecting.
    /// </summary>
    private CancellationTokenSource CancelTokenSource = new();

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

    /// <summary>
    /// Gets the current average frames per second.
    /// </summary>
    private float AvgFps
    {
        get
        {
            return frameNumber / (float)gameTimer.Elapsed.TotalSeconds;
        }
    }

    /// <summary>
    /// Starts the two core background loops, one for receiving Json objects from the server and
    /// the other rendering the world.
    /// </summary>
    private void GameLoop()
    {
        Task receiveLoop = RecieveLoop(CancelTokenSource.Token);
        Task renderLoop = RenderLoop(CancelTokenSource.Token);
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
    private void ConnectToServer()
    {
        new Task(() =>
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
                int worldSize = int.Parse(network.ReceiveLine());
                worldModel = new World(worldSize);
                GameLoop();

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
        }).Start();
        //Im pretty sure this is already done in the GameLoop() we call above.
        //Task gameloop = new(() => GameLoop());
        //gameloop.RunSynchronously();
    }

    /// <summary>
    /// Background thread that continuously receives lines of tet from the server. 
    /// Each line is JSON representing a wall, snake, or powerup.
    /// </summary>
    /// <param name="ct">CancellationToken used to stop the loop.</param>
    /// <returns>//What does this return</returns>
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

    /// <summary>
    /// Handles rendering the world to the canvas.
    /// </summary>
    /// <param name="ct">Cancellation token to stop the loop.</param>
    /// <returns>????</returns>
    private async Task RenderLoop(CancellationToken ct)
    {
        frameNumber = 0;
        gameTimer.Restart();
        while (network.IsConnected && !ct.IsCancellationRequested)
        {
            await Task.Delay(16, ct);
            await InvokeAsync(async () =>
            {
                await DrawFrame();
                frameNumber++;
            });
        }

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
            //await context.LineToAsync(s.Body[i].X, s.Body[i].Y);
            await context.MoveToAsync(s.Body[i].X, s.Body[i].Y);
        }
        await context.LineToAsync(s.Tail.X, s.Tail.Y);
        //await context.SetLineWidthAsync(10);
        //await context.SetStrokeStyleAsync("lime");
        //await context.StrokeAsync();
    }
}
