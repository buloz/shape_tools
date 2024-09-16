using System;
using System.Reflection;

public class Program
{
    public static void Main(string[] args)
    {
        if (args.Length < 2)
        {
            Console.WriteLine("Usage: dotnet run <method> <number_of_polygons>");
            return;
        }

        string method = args[0];
        int numberOfPolygons;

        if (!int.TryParse(args[1], out numberOfPolygons))
        {
            Console.WriteLine("The number of polygons must be a valid integer.");
            return;
        }

        ShapeGenerator generator = new ShapeGenerator();

        try
        {
            var methodInfo = typeof(ShapeGenerator).GetMethod(method, BindingFlags.IgnoreCase | BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public);
            
            if (methodInfo == null)
            {
                Console.WriteLine("Method not recognized. Use 'GeneratePolygons' or 'GeneratePolygon'.");
                return;
            }

            object result;
            if (methodInfo.IsStatic)
            {
                result = methodInfo.Invoke(null, new object[] { numberOfPolygons });
            }
            else
            {
                result = methodInfo.Invoke(generator, new object[] { });
            }

            Console.WriteLine(result);
            Clipboard.SetText(result.ToString());
            Console.WriteLine("Copied!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
    }
}