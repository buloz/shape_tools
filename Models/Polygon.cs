using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace ShapeTools.Models;

/// <summary>
/// Represents a polygon defined by a list of coordinates.
/// </summary>
public class Polygon
{

    /// <summary>
    /// Gets or sets the list of points that make up the polygon.
    /// </summary>
    public List<Coordinate> Points { get; set; }

#region ctor

    /// <summary>
    /// Initializes a new instance of the Polygon class with an empty list of points.
    /// </summary>
    public Polygon()
    {
        Points = new List<Coordinate>();
    }

    /// <summary>
    /// Initializes a new instance of the Polygon class with a specified list of points.
    /// </summary>
    /// <param name="points">The list of coordinates that define the polygon.</param>
    public Polygon(List<Coordinate> points)
    {
        Points = points;
    }

#endregion
}