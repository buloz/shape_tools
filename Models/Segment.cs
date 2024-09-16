namespace ShapeTools.Models;

public class Segment : Shape
{
    public Coordinate[] Coordinates = new Coordinate[2];

    public Segment(Coordinate[] coordinates)
    {
        Coordinates = coordinates;
    }
}