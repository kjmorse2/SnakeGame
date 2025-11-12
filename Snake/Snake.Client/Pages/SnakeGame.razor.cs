using System.Text.Json;
using CS3500.Networking;
using System.Threading.Tasks;

namespace CS3500.Snake.Client.Pages.SnakeGame;

using CS3500.Snake.Models;


public partial class SnakeGame
{

    private static void GameLoop(int playerId, World world)
    {
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
        spinner = "spinner";
        InvokeAsync(StateHasChanged); // update UI to show connection status

        _ = Task.Run(() =>
        {
            try
            {
                // Communicating with the server needs to be asynchronous so that the UI thread
                // can continue drawing. Thus, we run on a background Task.
                network.Connect(serverNameOrAddress, serverPort);
                spinner = string.Empty;
                InvokeAsync(StateHasChanged);
                // Send first message: the username
                Logger.LogInformation($"Connected to server, sending username: {userName}");
                network.SendLine(userName);
                int playerId = int.Parse(network.ReceiveLine());
                World world = new(int.Parse(network.ReceiveLine()));

                while (true)
                {
                    try
                    {
                        Wall receivedWall = JsonSerializer.Deserialize<Wall>(network.ReceiveLine()) ?? throw new NullReferenceException();
                        world.Walls[receivedWall.Id] = receivedWall;
                    }
                    catch (JsonException)
                    {
                        break;
                    }
                    catch (InvalidOperationException)
                    {
                        // TODO handle disconnect gracefully
                        break;
                    }
                    catch (NullReferenceException)
                    {
                        // TODO if you got here something else happened.
                        break;
                    }
                }

                GameLoop(playerId, world);
            }
            catch
            {
                noServerMessage = $"There is no server at: {serverNameOrAddress}, port: {serverPort}.";
                Logger.LogInformation(noServerMessage);
                spinner = string.Empty;
                InvokeAsync(StateHasChanged);
                // you can simulate this by trying to connect to a port where no server is running.
            }
        });
    }
}
