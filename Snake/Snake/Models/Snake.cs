namespace Snake.Models;

public class Snake
{
    /// <summary>
    /// Gets or sets the Id of the snake.
    /// </summary>
    public int snake { get; set; }

    /// <summary>
    /// Gets or sets the name of the player.
    /// </summary>
    public string name { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public List<Point2D> body { get; set; } = new();

    /// <summary>
    ///  
    /// </summary>
    public Point2D dir { get; set; } = new();

    /// <summary>
    /// 
    /// </summary>
    public int score { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public bool died { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public bool alive { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public bool dc { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public bool join { get; set; }
}
