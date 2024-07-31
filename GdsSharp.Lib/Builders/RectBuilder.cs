using GdsSharp.Lib.NonTerminals;
using GdsSharp.Lib.NonTerminals.Elements;

namespace GdsSharp.Lib.Builders;

/// <summary>
///     Helper class for creating rectangle elements.
/// </summary>
public static class RectBuilder
{
    /// <summary>
    ///     Helper function for creating a rectangle element.
    /// </summary>
    /// <param name="x">Lower left X coordinate.</param>
    /// <param name="y">Lower left Y coordinate.</param>
    /// <param name="width">Width of the rect.</param>
    /// <param name="height">Height of the rect.</param>
    /// <returns>Rectangular polygon (GdsBoundaryElement).</returns>
    public static GdsElement CreateRect(int x, int y, int width, int height)
    {
        var element = new GdsElement
        {
            Element = new GdsBoundaryElement
            {
                Points = new List<GdsPoint>
                {
                    new(x, y),
                    new(x + width, y),
                    new(x + width, y + height),
                    new(x, y + height),
                    new(x, y)
                }
            }
        };

        return element;
    }
}