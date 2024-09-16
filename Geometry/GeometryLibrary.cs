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
    public static Polygon Union(this Polygon polygon1, Polygon polygon2)
    {
        if (polygon1 == null || polygon2 == null)
        {
            throw new ArgumentNullException("Polygons cannot be null");
        }
        List<Coordinate> intersections = polygon1.Intersects(polygon2);
        var points = polygon1.Points.Where(p => !p.isInside(polygon2))
            .Concat(polygon2.Points.Where(p => !p.isInside(polygon1)))
            .Concat(intersections)
            .OrderBy(p => Math.Atan2(p.Y - polygon1.Points[0].Y, p.X - polygon1.Points[0].X))
            .Distinct()
            .ToList();
        return new Polygon(points);
    }

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