namespace ShapeTools.Models;

/// <summary>
/// Represents a line segment defined by two coordinates.
/// </summary>
public struct Segment
{

    /// <summary>
    /// Gets or sets the array of coordinates defining the segment.
    /// </summary>
    public Coordinate[] Coordinates = new Coordinate[2];

#region ctor

    /// <summary>
    /// Initializes a new instance of the Segment class with default coordinates.
    /// </summary>
    public Segment()
    {
        Coordinates[0] = new Coordinate();
        Coordinates[1] = new Coordinate();
    }

    /// <summary>
    /// Initializes a new instance of the Segment class with the specified coordinates.
    /// </summary>
    /// <param name="coordinates">An array of two coordinates defining the segment.</param>
    public Segment(Coordinate[] coordinates)
    {
        Coordinates = coordinates;
    }

#endregion
}