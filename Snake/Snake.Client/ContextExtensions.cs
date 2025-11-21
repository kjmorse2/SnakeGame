// <copyright file="ContextExtensions.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using Blazor.Extensions.Canvas.Canvas2D;
using Point2D = CS3500.Snake.Models.Point2D;
using PowerUp = CS3500.Snake.Models.PowerUp;
using Wall = CS3500.Snake.Models.Wall;

namespace Snake.Client;

/// <summary>
///     Canvas drawing helper extensions for rendering world elements using a <see cref="Canvas2DContext" />.
/// </summary>
public static class ContextExtensions
{
    /// <summary>
    ///     Array of colors to for the snake to be drawn in .
    /// </summary>
    private static readonly string[ ] SnakeColors =
    [
        "lime", "cyan", "yellow", "orange", "magenta", "red", "blue", "white",
    ];

    private static readonly float[ ][ ] SnakePatterns =
    [
        [ ], // solid
        [ 7, 7 ], [ 4, 4 ], [ 15, 5 ], [ 3, 1, 4 ],
        [ 15, 3, 3, 3 ], [ 20, 3, 3, 3, 3, 3, 3, 3 ], [ 12, 3, 3 ],
    ];

    /// <summary>
    ///     Draws a collection of snakes by delegating to <see cref="Draw(Canvas2DContext, Snake)" />.
    /// </summary>
    /// <param name="context">The canvas 2D context to draw with.</param>
    /// <param name="snakes">The sequence of snakes to render.</param>
    /// <returns>>A task representing the asynchronous operation.</returns>
    public static async Task Draw(this Canvas2DContext context, IEnumerable<CS3500.Snake.Models.Snake> snakes)
    {
        foreach (CS3500.Snake.Models.Snake snake in snakes)
        {
            await context.Draw(snake);
        }
    }

    /// <summary>
    ///     Draws a collection of power-ups, skipping those marked dead.
    /// </summary>
    /// <param name="context">The canvas 2D context to draw with.</param>
    /// <param name="powerUps">The sequence of power-ups to render.</param>
    /// <returns>>A task representing the asynchronous operation.</returns>
    public static async Task Draw(this Canvas2DContext context, IEnumerable<PowerUp> powerUps)
    {
        foreach (PowerUp powerUp in powerUps)
        {
            await context.SetFillStyleAsync("yellow");
            await context.Draw(powerUp);
        }
    }

    /// <summary>
    ///     Draws a list of walls onto the canvas by rendering each wall's segments.
    /// </summary>
    /// <param name="context">The context to draw each of the walls onto.</param>
    /// <param name="walls">This list of walls to draw.</param>
    /// <returns>>A task representing the asynchronous operation.</returns>
    public static async Task Draw(this Canvas2DContext context, IEnumerable<Wall> walls)
    {
        await context.SetFillStyleAsync("red");
        foreach (Wall wall in walls)
        {
            await context.Draw(wall);
        }
    }

    /// <summary>
    ///     Draws a single snake as a stroked polyline from tail to head.
    /// </summary>
    /// <param name="context">The canvas 2D context to draw with.</param>
    /// <param name="snake">The snake to render.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private static async Task Draw(this Canvas2DContext context, CS3500.Snake.Models.Snake snake)
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
            await context.LineToAsync(snake.Body[ i ].X, snake.Body[ i ].Y);
        }

        await context.LineToAsync(snake.Head.X, snake.Head.Y);

        // Set a color
        await context.SetStrokeStyleAsync(SnakeColors[ snake.Id % SnakeColors.Length ]);

        // Set a dash pattern
        await context.SetLineDashAsync(SnakePatterns[ snake.Id % SnakePatterns.Length ]);
        await context.StrokeAsync();

        await context.SetFontAsync("14px Arial");
        await context.SetFillStyleAsync("white");
        await context.FillTextAsync($" {snake.Name}:{snake.Score}", snake.Head.X - 20, snake.Head.Y + 30);

        // Restore previous stroke thickness
        await context.SetLineWidthAsync(oldLineWidth);
        await context.SetLineDashAsync([ ]);
    }

    /// <summary>
    ///     Draws a single power-up as a filled square centered on its position.
    /// </summary>
    /// <param name="context">The canvas 2D context to draw with.</param>
    /// <param name="powerUp">The power-up to render.</param>
    /// <returns>>A task representing the asynchronous operation.</returns>
    private static async Task Draw(this Canvas2DContext context, PowerUp powerUp)
    {
        await context.FillRectAsync(powerUp.Position.X - 8, powerUp.Position.Y - 8, 16, 16);
    }

    /// <summary>
    ///     Draws a single wall by rendering each of its segments as an image.
    /// </summary>
    /// <param name="context">The context to draw the wall onto.</param>
    /// <param name="wall">The wall object parsed from json to draw.</param>
    /// <returns>>A task representing the asynchronous operation.</returns>
    private static async Task Draw(this Canvas2DContext context, Wall wall)
    {
        foreach (Point2D segment in wall.GetSegments())
        {
            await context.DrawImageAsync(
                Wall.Wall_Image_Refernce,
                segment.X - 25,
                segment.Y - 25,
                Wall.SegmentSize,
                Wall.SegmentSize);

            // await context.FillRectAsync(segment.X - 25, segment.Y - 25, Wall.SegmentSize, Wall.SegmentSize);
        }
    }
}
