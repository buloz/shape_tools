using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using ShapeTools.Models;

namespace ShapeTools.Tools;

public static class Serialization
{
    /// <summary>
    /// Deserializes a list of polygons from a GeoJSON file.
    /// </summary>
    /// <param name="fileName">The name of the GeoJSON file to read.</param>
    /// <returns>A list of deserialized polygons.</returns>
    public static List<Polygon> DeserializePolygons(string filePath)
    {
        string jsonContent = File.ReadAllText(filePath);
        
        var options = new JsonSerializerOptions
        {
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            PropertyNameCaseInsensitive = true
        };

        var geoJson = JsonSerializer.Deserialize<GeoJsonFeatureCollection>(jsonContent, options);
        
        List<Polygon> polygons = new List<Polygon>();
        foreach (var feature in geoJson.Features)
        {
            if (feature.Geometry.Type == "Polygon")
            {
                List<Coordinate> coordinates = feature.Geometry.Coordinates[0]
                    .Select(coord => new Coordinate(coord[0], coord[1]))
                    .ToList();
                polygons.Add(new Polygon(coordinates));
            }
        }

        return polygons;
    }

    private class GeoJsonFeatureCollection
    {
        public List<GeoJsonFeature> Features { get; set; }
    }

    private class GeoJsonFeature
    {
        public GeoJsonGeometry Geometry { get; set; }
    }

    private class GeoJsonGeometry
    {
        public string Type { get; set; }
        public List<List<List<double>>> Coordinates { get; set; }
    }
}
