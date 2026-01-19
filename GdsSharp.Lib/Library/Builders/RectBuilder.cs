namespace GdsSharp.Lib.Library.Builders;

/// <summary>
///     Helper class for creating rectangle elements.
/// </summary>
public static class RectBuilder
{
    /// <summary>
    ///     Helper function for creating a rectangle polygon.
    /// </summary>
    /// <param name="x">Lower left X coordinate.</param>
    /// <param name="y">Lower left Y coordinate.</param>
    /// <param name="width">Width of the rect.</param>
    /// <param name="height">Height of the rect.</param>
    /// <returns>Array of 5 points representing a closed rectangular polygon.</returns>
    public static GdsPoint[] CreateRectPoints(int x, int y, int width, int height)
    {
        return
        [
            new GdsPoint(x, y),
            new GdsPoint(x + width, y),
            new GdsPoint(x + width, y + height),
            new GdsPoint(x, y + height),
            new GdsPoint(x, y)
        ];
    }
}