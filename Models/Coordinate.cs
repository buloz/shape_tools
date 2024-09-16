namespace ShapeTools.Models;

/// <summary>
/// Represents a coordinate in a two-dimensional space with X and Y components.
/// </summary>
public class Coordinate
{
    /// <summary>
    /// Gets or sets the X component of the coordinate.
    /// </summary>
    public double X { get; set; }

    /// <summary>
    /// Gets or sets the Y component of the coordinate.
    /// </summary>
    public double Y { get; set; }

#region ctor

    /// <summary>
    /// Initializes a new instance of the Coordinate class with default values (0, 0).
    /// </summary>
    public Coordinate()
    {
        X = 0.0;
        Y = 0.0;
    }

    /// <summary>
    /// Initializes a new instance of the Coordinate class with the specified X and Y values.
    /// </summary>
    /// <param name="x">The value of the X component.</param>
    /// <param name="y">The value of the Y component.</param>
    public Coordinate(double x, double y)
    {
        X = x;
        Y = y;
    }

#endregion
}