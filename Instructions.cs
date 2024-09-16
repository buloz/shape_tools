using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipelines;
using System.Text.Json;
using ShapeTools.Geometry;
using ShapeTools.Models;
using ShapeTools.Tools;


/// <summary>
/// Class containing static methods for generating polygons and performing geometric operations.
/// This class provides functionality to create random polygons and perform polygon unions.
/// </summary>
public static class Instructions
{
    /// <summary>
    /// Generates a specified number of polygons using the ShapeGenerator.
    /// </summary>
    /// <param name="numberOfPolygons">The number of polygons to generate.</param>
    public static string GeneratePolygons(string numberOfPolygons)
    {
        return ShapeGenerator.GeneratePolygons(int.Parse(numberOfPolygons));
    }

    /// <summary>
    /// Generates a single polygon using the ShapeGenerator.
    /// </summary>
    public static string GeneratePolygon()
    {
        return new List<Polygon> { ShapeGenerator.GeneratePolygon() }.ToGeojson();
    }

    /// <summary>
    /// Performs a union operation on two polygons read from a specified file.
    /// </summary>
    /// <param name="fileName">The name of the file containing the polygons.</param>
    public static string Union(string fileName)
    {
        List<Polygon> polygons = Serialization.DeserializePolygons(Path.Combine(Directory.GetCurrentDirectory(), $"Resources/{fileName}.geojson"));
        return new List<Polygon> { polygons[0].Union(polygons[1]) }.ToGeojson();
    }
}