using System.Drawing;
using System.Globalization;
using System.Numerics;
using ShapeTools.Models;

namespace ShapeTools.Geometry;
/// <summary>
/// Class providing geometric operations on polygons.
/// </summary>
public static class GeometryLibrary
{
    /// <summary>
    /// Performs a union operation between two polygons.
    /// </summary>
    /// <param name="polygon1">The first polygon.</param>
    /// <param name="polygon2">The second polygon.</param>
    /// <returns>A new polygon resulting from the union of the two input polygons.</returns>
    /// <exception cref="ArgumentNullException">Thrown if either polygon is null.</exception>
    public static Polygon UnionGreinerHormann(this Polygon polygon1, Polygon polygon2)
    {
        if (polygon1 == null || polygon2 == null)
        {
            throw new ArgumentNullException("Polygons cannot be null");
        }
        List<Coordinate> intersections = polygon1.Intersects(polygon2);
        var points = polygon1.Points.Where(p => !p.isInside(polygon2))
            .Concat(intersections)
            .OrderBy(p => Math.Atan2(p.Y - polygon1.Points[0].Y, p.X - polygon1.Points[0].X))
            .Concat((polygon2.Points.Where(p => !p.isInside(polygon1))
            .Concat(intersections)
            .OrderBy(p => Math.Atan2(p.Y - polygon2.Points[0].Y, p.X - polygon2.Points[0].X))))
            .Distinct()
            .ToList();
        return new Polygon(points);
    }

#region Graph
    public static Polygon UnionByGraph(this Polygon polygon1, Polygon polygon2)
    {
        if (polygon1 == null || polygon2 == null)
        {
            throw new ArgumentNullException("Polygons cannot be null");
        }

        polygon1.IntersectsDetails(polygon2);

        Graph graph = new()
        {
            Root = new Node()
            {
                Self = polygon1.Points[0],
                Children = new List<Node>(),
            },
            KeyToNode = new Dictionary<double, Node>()
        };

        graph.KeyToNode.Add(Graph.CoordinateToKey(polygon1.Points[0]), graph.Root);

        Node currentNode = graph.Root;

        foreach (var point in polygon1.Points.Skip(1))
        {
            currentNode.Children.Add(new Node()
            {
                Self = point,
                Children = new List<Node>(),
            });
            //todo: check if key exists
            graph.KeyToNode.Add(Graph.CoordinateToKey(point), currentNode.Children.Last());
            currentNode = currentNode.Children.Last();
        }

        currentNode.Children.Add(graph.Root);
        InsertInGraph(polygon2, graph);

        return graph.ToPolygon();
    }

    private static Node InsertInGraph(Polygon polygon2, Graph graph)
    {
        Node currentNode;
        double key = Graph.CoordinateToKey(polygon2.Points[0]);
        double storedKey = key;
        if (graph.KeyToNode.ContainsKey(key))
        {
            currentNode = graph.KeyToNode[key];
        }
        else
        {
            currentNode = new Node()
            {
                Self = polygon2.Points[0],
                Children = new List<Node>(),
            };
            graph.KeyToNode.Add(key, currentNode);
        }

        for (int i = 1; i < polygon2.Points.Count + 1; i++)
        {
            var point = polygon2.Points[i % polygon2.Points.Count];
            Node nextNode;
            key = Graph.CoordinateToKey(point);
            if (graph.KeyToNode.ContainsKey(key))
            {
                nextNode = graph.KeyToNode[key];
                currentNode.Children.Add(nextNode);
            }
            else
            {
                currentNode.Children.Add(new Node()
                {
                    Self = point,
                    Children = new List<Node>()
                });
                nextNode = currentNode.Children.Last();
                graph.KeyToNode.Add(key, nextNode);
            }
            currentNode = nextNode;
        }

        double storedKey2 = Graph.CoordinateToKey(currentNode.Self);
        if (storedKey == storedKey2)
        {
            return currentNode;
        }

        return currentNode;
    }
#endregion Graph
    /// <summary>
    /// Finds the intersection points between two polygons.
    /// </summary>
    /// <param name="polygon1">The first polygon.</param>
    /// <param name="polygon2">The second polygon.</param>
    /// <returns>A list of coordinates representing the intersection points.</returns>
    public static List<Coordinate> Intersects(this Polygon polygon1, Polygon polygon2)
    {
        List<Coordinate> intersections = new List<Coordinate>();
        for (int i = 0; i < polygon1.Points.Count; i++)
        {
            var segment1 = new Segment(new Coordinate[] { polygon1.Points[i], polygon1.Points[(i + 1) % polygon1.Points.Count] });
            for (int j = 0; j < polygon2.Points.Count; j++)
            {
                var segment2 = new Segment(new Coordinate[] { polygon2.Points[j], polygon2.Points[(j + 1) % polygon2.Points.Count] });
                if (DetectCollision(segment1, segment2, out Coordinate intersectionPoint))
                {
                    intersections.Add(intersectionPoint);
                }
            }
        }
        return intersections;
    }

    public static void IntersectsDetails(this Polygon polygon1, Polygon polygon2)      
    {
        for (int i = 0; i < polygon1.Points.Count; i++)
        {
            var segment1 = new Segment(new Coordinate[] { polygon1.Points[i], polygon1.Points[(i + 1) % polygon1.Points.Count] });
            for (int j = 0; j < polygon2.Points.Count; j++)
            {
                var segment2 = new Segment(new Coordinate[] { polygon2.Points[j], polygon2.Points[(j + 1) % polygon2.Points.Count] });
                if (DetectCollision(segment1, segment2, out Coordinate intersectionPoint))
                {
                    polygon1.Points.Insert(i + 1, intersectionPoint);
                    polygon2.Points.Insert(j + 1, intersectionPoint);
                    i++;
                    j++;
                }
            }
        }
    }

    /// <summary>
    /// Detects a collision between two segments and calculates the intersection point.
    /// </summary>
    /// <param name="segment1">The first segment.</param>
    /// <param name="segment2">The second segment.</param>
    /// <param name="collisionPoint">The intersection point, if a collision is detected.</param>
    /// <returns>True if a collision is detected, otherwise False.</returns>
    private static bool DetectCollision(Segment segment1, Segment segment2, out Coordinate collisionPoint)
    {
        collisionPoint = new Coordinate();

        double x1 = segment1.Coordinates[0].X;
        double y1 = segment1.Coordinates[0].Y;
        double x2 = segment1.Coordinates[1].X;
        double y2 = segment1.Coordinates[1].Y;
        double x3 = segment2.Coordinates[0].X;
        double y3 = segment2.Coordinates[0].Y;
        double x4 = segment2.Coordinates[1].X;
        double y4 = segment2.Coordinates[1].Y;

        double denom = (y4 - y3) * (x2 - x1) - (x4 - x3) * (y2 - y1);
        if (Math.Abs(denom) < 1e-8)
        {
            return false; // The segments are parallel
        }

        double ua = ((x4 - x3) * (y1 - y3) - (y4 - y3) * (x1 - x3)) / denom;
        double ub = ((x2 - x1) * (y1 - y3) - (y2 - y1) * (x1 - x3)) / denom;

        if (ua < 0 || ua > 1 || ub < 0 || ub > 1)
        {
            return false; // The intersection is outside the segments
        }

        double x = x1 + ua * (x2 - x1);
        double y = y1 + ua * (y2 - y1);

        collisionPoint = new Coordinate(x, y);
        return true;
    }

    /// <summary>
    /// Checks if a point is inside a polygon using the ray-casting algorithm.
    /// </summary>
    /// <param name="point">The point to check.</param>
    /// <param name="polygon">The polygon to test against.</param>
    /// <returns>True if the point is inside the polygon, false otherwise.</returns>
    private static bool isInside(this Coordinate point, Polygon polygon)
    {
        bool isInside = false;
        int j = polygon.Points.Count - 1;

        for (int i = 0; i < polygon.Points.Count; i++)
        {
            if (((polygon.Points[i].Y > point.Y) != (polygon.Points[j].Y > point.Y)) &&
                (point.X < (polygon.Points[j].X - polygon.Points[i].X) * (point.Y - polygon.Points[i].Y) / (polygon.Points[j].Y - polygon.Points[i].Y) + polygon.Points[i].X))
            {
                isInside = !isInside;
            }
            j = i;
        }

        return isInside;
    }
}

public struct Intersection
{
    public Coordinate Point { get; set; }
    public (int p1, int p2) Segment1 { get; set; }
    public (int p1, int p2) Segment2 { get; set; }
}

public struct Node 
{
    public Coordinate Self { get; set; }

    public List<Node> Children { get; set; }
}

public struct Graph
{
    public Node Root { get; set; }

    public Dictionary<double, Node> KeyToNode { get; set; }

    public static double CoordinateToKey(Coordinate coordinate)
    {
        return coordinate.X + coordinate.Y * 180;
    }

    public Polygon ToPolygon()
    {
        var polygon = new Polygon();
        var currentNode = Root;
        var rootKey = CoordinateToKey(Root.Self);
        polygon.Points.Add(currentNode.Self);
        var vecCurrent = new Vector2(0,1);
        while (true)
        {
            int maxIndex = 0;
            float maxAngle = -1.0f;

            for (int i = 0; i < currentNode.Children.Count; i++)
            {
                var vecNext = new Vector2(
                    (float)(currentNode.Self.X - currentNode.Children[i].Self.X), 
                    (float)(currentNode.Self.Y - currentNode.Children[i].Self.Y));
                var crossProduct = vecCurrent.X * vecNext.Y - vecCurrent.Y * vecNext.X;
                float angle = Vector2.Dot(Vector2.Normalize(vecCurrent), Vector2.Normalize(vecNext)) * 90 + 90;
                angle = crossProduct < 0 ? 360 - angle : angle;
                if (angle > maxAngle)
                {
                    maxIndex = i;
                    maxAngle = angle;
                }
            }
            

            var storedKey = Graph.CoordinateToKey(currentNode.Children[maxIndex].Self);
            
            if(Graph.CoordinateToKey(currentNode.Children[maxIndex].Self) == rootKey)
            {
                break;
            }
            
            vecCurrent = new Vector2(
                (float)(currentNode.Self.X - currentNode.Children[0].Self.X), 
                (float)(currentNode.Self.Y - currentNode.Children[0].Self.Y));
            currentNode = currentNode.Children[maxIndex];
            polygon.Points.Add(currentNode.Self);
        }

        return polygon;
    }
}