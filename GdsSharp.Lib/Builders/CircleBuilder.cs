using GdsSharp.Lib.NonTerminals;
using GdsSharp.Lib.NonTerminals.Elements;

namespace GdsSharp.Lib.Builders;

/// <summary>
///     Helper class for constructing circles.
/// </summary>
public static class CircleBuilder
{
    /// <summary>
    ///     Creates a polygon approximation of a circle.
    /// </summary>
    /// <param name="x">Center X coordinate.</param>
    /// <param name="y">Center Y coordinate.</param>
    /// <param name="radius">Radius of the circle.</param>
    /// <param name="numPoints">A multiple of 8.</param>
    /// <returns>Polygon approximating a circle.</returns>
    public static GdsElement CreateCircle(int x, int y, int radius, int numPoints = 64)
    {
        if (numPoints % 8 != 0)
            throw new ArgumentException("Number of points must be a multiple of 8.", nameof(numPoints));

        var pointsPerSector = new Dictionary<int, List<GdsPoint>>
        {
            { 0, new() },
            { 1, new() },
            { 2, new() },
            { 3, new() },
            { 4, new() },
            { 5, new() },
            { 6, new() },
            { 7, new() }
        };

        var currentX = 0;
        var currentY = radius;
        var d = 3 - 2 * radius;
        WriteSymmetricPoints(x, y, currentX, currentY);

        while (currentY > currentX)
        {
            currentX++;
            if (d > 0)
            {
                currentY--;
                d = d + 4 * (currentX - currentY) + 10;
            }
            else
            {
                d = d + 4 * currentX + 6;
            }

            WriteSymmetricPoints(x, y, currentX, currentY);
        }

        var element = new GdsElement
        {
            Element = new GdsBoundaryElement
            {
                Points = pointsPerSector
                    .Select(kvp => kvp.Key % 2 == 0 ? kvp.Value : kvp.Value.Reverse<GdsPoint>())
                    .SelectMany(p => SampleEquidistant(p, numPoints / 8))
                    .ToList(),
                NumPoints = numPoints
            }
        };

        return element;

        void WriteSymmetricPoints(int xc, int yc, int x, int y)
        {
            pointsPerSector[0].Add(new GdsPoint(xc + x, yc + y));
            pointsPerSector[1].Add(new GdsPoint(xc + y, yc + x));
            pointsPerSector[2].Add(new GdsPoint(xc + y, yc - x));
            pointsPerSector[3].Add(new GdsPoint(xc + x, yc - y));
            pointsPerSector[4].Add(new GdsPoint(xc - x, yc - y));
            pointsPerSector[5].Add(new GdsPoint(xc - y, yc - x));
            pointsPerSector[6].Add(new GdsPoint(xc - y, yc + x));
            pointsPerSector[7].Add(new GdsPoint(xc - x, yc + y));
        }
    }

    private static IEnumerable<T> SampleEquidistant<T>(IEnumerable<T> source, int numSamples)
    {
        var sourceList = source.ToList();
        var step = (sourceList.Count - 1) / Math.Max(numSamples, 1);
        return Enumerable.Range(0, numSamples).Select(i => sourceList[i * step]);
    }
}