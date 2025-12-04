// <copyright file="World.cs" company="U of U CS3500">
// Copyright (c) U of U CS3500, Kenneth Morse, and Hunter Simmons. All rights reserved.
// </copyright>

using System.Collections.Concurrent;
using System.Text.Json;
using CS3500.Networking;

namespace CS3500.SnakeModels;

/// <summary>
///     <p>
///         Represents the authoritative state of the game world for a single client: square boundary size and
///         collections of snakes, walls, and power-ups keyed by unique IDs. Collections are concurrent to allow
///         safe updates from a background receive loop while rendering.
///     </p>
///     <p>
///         Worlds responsibilities include:
///     </p>
///     <list type="bullet">
///         <item> Maintaining collections of game elements (snakes, walls, power-ups).</item>
///         <item> Deserializing objects and applying JSON updates for game elements.</item>
///         <item> Tracking which snakes and power-ups need to be removed from the world.</item>
///         <item> Interfacing with the database for player score tracking.</item>
///     </list>
/// </summary>
public class World
{
    /// <summary>
    ///     A database interface for the Snake game, stores game information on a remove SQL server.
    /// </summary>
    private readonly DatabaseInterface dbInterface = new();

    /// <summary>
    ///     Initializes a new instance of the <see cref="World" /> class with a square size in pixels.
    /// </summary>
    /// <param name="size">Square dimension (width == height) of the playable area.</param>
    public World(int size)
    {
        // Set the world size and notify the database of a new game.
        Size = size;

        // Initialize a new game in the database.
        dbInterface.NewGame();
    }

    /// <summary>
    ///     Gets the collection of power-ups keyed by id.
    /// </summary>
    public ConcurrentDictionary<int, PowerUp> PowerUps { get; } = new();

    /// <summary>
    ///     Gets the world square size in pixels.
    /// </summary>
    public int Size { get; }

    /// <summary>
    ///     Gets the collection of snakes keyed by id.
    /// </summary>
    public ConcurrentDictionary<int, Snake> Snakes { get; } = new();

    /// <summary>
    ///     Gets the collection of walls keyed by id.
    /// </summary>
    public ConcurrentDictionary<int, Wall> Walls { get; } = new();

    /// <summary>
    ///     Gets or sets a value indicating whether the walls have been loaded.
    /// </summary>
    public bool WallsLoaded { get; set; }

    private static Point2D DefaultPoint => new() { X = 0, Y = 0 };

    /// <summary>
    ///     Gets a collection of power-up IDs to be removed from the world.
    /// </summary>
    private ConcurrentBag<int> RemovePowerUpIDs { get; } = new();

    /// <summary>
    ///     Gets a collection of snake IDs to be removed from the world.
    /// </summary>
    private ConcurrentBag<int> RemoveSnakeIds { get; } = new();

    /// <summary>
    ///     Removes dead snakes and consumed power-ups from the snakes and power-ups lists.
    /// </summary>
    public void CleanupDeadElements()
    {
        while (RemoveSnakeIds.TryTake(out int snakeId))
        {
            Snakes.TryRemove(snakeId, out _);
        }

        while (RemovePowerUpIDs.TryTake(out int powerUpId))
        {
            PowerUps.TryRemove(powerUpId, out _);
        }
    }

    /// <summary>
    ///     Clears all world elements from the collections.
    /// </summary>
    public void Clear()
    {
        // Notify the database that the game has ended.
        dbInterface.EndGame();

        // Clear all collections.
        Snakes.Clear();
        Walls.Clear();
        PowerUps.Clear();
    }

    /// <summary>
    ///     Gets the head position of the snake with the given player ID.
    /// </summary>
    /// <param name="playerId">The player ID of the snake to get the head for.</param>
    /// <returns>A 2D point representing the head.</returns>
    public Point2D GetHead(int playerId)
    {
        if (Snakes.TryGetValue(playerId, out Snake? snake))
        {
            return snake.Head;
        }

        return DefaultPoint;
    }

    /// <summary>
    ///     Deserializes and applies a JSON update for a single game element (snake, power-up, wall).
    ///     The element type is inferred from the 3rd character of the JSON string (index 2).
    /// </summary>
    /// <param name="jsonString">JSON payload representing an update from the server.</param>
    /// <remarks>
    ///     Expected leading type markers:
    ///     's' => <see cref="Snake" />, 'p' => <see cref="PowerUp" />, 'w' => <see cref="Wall" />.
    /// </remarks>
    public void UpdateElement(string jsonString)
    {
        // If the JSON string is empty, don't do any work.
        if (string.IsNullOrWhiteSpace(jsonString))
        {
            return;
        }

        // The second character of the JSON string is the start of the object, there are 3 distinct cases.
        // 's' = snake, parse a snake object.
        // 'w' = wall, parse a wall object.
        // 'p' = power-up, parse a power up object.
        char type = jsonString[ 2 ];
        try
        {
            switch (type)
            {
                case 's':
                    Snake receivedSnake = JsonSerializer.Deserialize<Snake>(jsonString) ??
                                          throw new InvalidOperationException();

                    // Update the database with the new player or updated score.
                    if (!Snakes.TryGetValue(receivedSnake.Id, out Snake? oldSnake))
                    {
                        // If the snake is new, initialize MaxScore and insert into the database.
                        receivedSnake.MaxScore = receivedSnake.Score;
                        dbInterface.InsertNewPlayer(receivedSnake.Id, receivedSnake.Name, receivedSnake.Score);
                    }
                    else
                    {
                        // Preserve the MaxScore from the old snake and update if current score is higher.
                        receivedSnake.MaxScore = oldSnake.MaxScore;
                        if (receivedSnake.Score > receivedSnake.MaxScore)
                        {
                            receivedSnake.MaxScore = receivedSnake.Score;
                            dbInterface.UpdatePlayerScore(receivedSnake.Id, receivedSnake.Score, oldSnake.MaxScore);
                        }
                    }

                    Snakes[ receivedSnake.Id ] = receivedSnake;

                    // In the case the snake disconnects, remove it from the world.
                    if (receivedSnake.Dc)
                    {
                        // Notify the database that the player has left.
                        dbInterface.PlayerLeft(receivedSnake.Id);
                        RemoveSnakeIds.Add(receivedSnake.Id);
                    }

                    break;
                case 'p':
                    PowerUp receivedPowerUp = JsonSerializer.Deserialize<PowerUp>(jsonString) ??
                                              throw new InvalidOperationException();
                    PowerUps[ receivedPowerUp.Id ] = receivedPowerUp;

                    // In the case the power-up is dead, remove it from the list.
                    if (receivedPowerUp.IsDead)
                    {
                        RemovePowerUpIDs.Add(receivedPowerUp.Id);
                    }

                    break;
                case 'w':
                    Wall receivedWall = JsonSerializer.Deserialize<Wall>(jsonString) ??
                                        throw new InvalidOperationException();
                    Walls[ receivedWall.Id ] = receivedWall;
                    break;
            }
        }
        catch (JsonException e)
        {
            throw new InvalidOperationException("Failed to deserialize JSON string.", e);
        }
        catch (Exception e)
        {
            throw new InvalidOperationException("An error occurred while updating the world element.", e);
        }
    }
}
