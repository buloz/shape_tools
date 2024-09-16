using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace ShapeTools.Models;

public class Polygon : Shape
{
    public List<Coordinate> Points { get; set; }

    public Polygon(List<Coordinate> points)
    {
        Points = points;
    }
}