using GdsSharp.Lib.Abstractions;

namespace GdsSharp.Lib.NonTerminals.Elements;

public class GdsElement
{
    public required IGdsElement Element { get; set; }
    public List<GdsProperty> Properties { get; set; } = new();

    /// <summary>
    /// Creates a polygon approximation of a circle.
    /// </summary>
    /// <param name="x">Center X coordinate.</param>
    /// <param name="y">Center Y coordinate.</param>
    /// <param name="radius">Radius of the circle.</param>
    /// <param name="sides">Number of sides of the generated polygon.</param>
    /// <returns>Polygon approximating a circle.</returns>
    public static GdsElement CreateCircle(int x, int y, int radius, int sides = 64)
    {
        var points = new List<GdsPoint>();
        for (var i = 0; i < sides; i++)
        {
            var angle = 2 * Math.PI / sides * i;
            var px = x + (int)Math.Round(radius * Math.Cos(angle));
            var py = y + (int)Math.Round(radius * Math.Sin(angle));
            points.Add(new GdsPoint { X = px, Y = py });
        }

        var element = new GdsElement
        {
            Element = new GdsBoundaryElement
            {
                Points = points,
            }
        };
        
        return element;
    }
}