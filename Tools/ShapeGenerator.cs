
using System;
using System.Text.Json;

namespace ShapeTools.Tools;

public class ShapeGenerator
{
    private static Random random = new Random();

    public static string GeneratePolygons(int numberOfPolygons)
    {
        List<Polygon> polygons = new List<Polygon>();
        for (int i = 0; i < numberOfPolygons; i++)
        {
            polygons.Add(GeneratePolygon());
        }
        return polygons.ToGeojson();
    }

    public static Polygon GeneratePolygon()
    {
        int numberOfPoints = random.Next(20, 100); // Between 20 and 100 points for the polygon
        List<Coordinate> points = new List<Coordinate>();

        // Generate the center of the polygon
        double centerX = random.Next(-180, 181);
        double centerY = random.Next(-90, 91);

        // Generate points around the center
        // Generate a base shape
        double baseRadius = random.Next(5, 61); // Base radius between 5 and 60
        List<double> radii = new List<double>();
        
        for (int i = 0; i < numberOfPoints; i++)
        {
            double angle = 2 * Math.PI * i / numberOfPoints;
            double radiusVariation = random.NextDouble() * 0.2 + 0.9; // Variation between 0.9 and 1.1
            double radius = baseRadius * radiusVariation;
            radii.Add(radius);
        }

        // Smoothing for more natural shapes
        for (int i = 0; i < numberOfPoints; i++)
        {
            int prev = (i - 1 + numberOfPoints) % numberOfPoints;
            int next = (i + 1) % numberOfPoints;
            radii[i] = (radii[prev] + radii[i] + radii[next]) / 3;
        }

        // Generate points
        for (int i = 0; i < numberOfPoints; i++)
        {
            double angle = 2 * Math.PI * i / numberOfPoints;
            double x = centerX + radii[i] * Math.Cos(angle);
            double y = centerY + radii[i] * Math.Sin(angle);
            
            // Ensure coordinates are within valid limits
            x = Math.Max(-180, Math.Min(180, x));
            y = Math.Max(-90, Math.Min(90, y));
            
            points.Add(new Coordinate((int)Math.Round(x), (int)Math.Round(y)));
        }

        // Add the first point at the end to close the polygon
        points.Add(points[0]);

        return new Polygon(points);
    }
}

public static class GeneratorExtension
{
    private static Random random = new Random();

    public static string ToGeojson(this List<Polygon> polygons)
    {
        var features = polygons.Select(polygon =>
        {
            string color = $"#{random.Next(0x80, 0xFF):X2}{random.Next(0x80, 0xFF):X2}{random.Next(0x80, 0xFF):X2}";
            return new
            {
                type = "Feature",
                geometry = new
                {
                    type = "Polygon",
                    coordinates = new List<List<double[]>> { polygon.Points.Select(p => new double[] { p.X, p.Y }).ToList() }
                },
                properties = new Dictionary<string, object>
                {
                    { "fill", color },
                    { "stroke", color },
                    { "fill-opacity", 0.4 },
                    { "stroke-width", 2 }
                },
            };
        }).ToList();

        var geojson = new
        {
            type = "FeatureCollection",
            features = features
        };

        var options = new JsonSerializerOptions
        {
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            WriteIndented = true
        };
        return JsonSerializer.Serialize(geojson, options);
    }
}
