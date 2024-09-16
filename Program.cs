using System;
using System.Reflection;
using ShapeTools.Models;
using ShapeTools.Tools;

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

        try
        {
            var methodInfo = typeof(Instructions).GetMethod(method, BindingFlags.IgnoreCase | BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public);
            
            if (methodInfo == null)
            {
                Console.WriteLine("Method not recognized. Use 'GeneratePolygons' or 'GeneratePolygon' or 'Union'.");
                return;
            }

            object result;
            if (methodInfo.IsStatic)
            {
                result = methodInfo.Invoke(null, new object[] { args[1] });
            }
            else
            {
                result = null;
                Console.WriteLine("Method not recognized. Use 'GeneratePolygons' or 'GeneratePolygon' or 'Union'.");
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