using System.Numerics;
using GdsSharp.Lib.NonTerminals;
using GdsSharp.Lib.NonTerminals.Elements;

namespace GdsSharp.Benchmarks.PathBuilder.Implementations;

public class PathBuilderSpan : PathBuilderBase
{
    public PathBuilderSpan(float initialWidth, Vector2? initialPosition = null, Vector2? initialHeading = null) : base(initialWidth, initialPosition, initialHeading)
    {
    }

    public override IEnumerable<GdsElement> Build(int maxVertices = 200)
    {
        if (maxVertices < 4)
            throw new ArgumentException("maxVerticesPerElement must be at least 4 to form a valid polygon.", nameof(maxVertices));

        var allPoints = GetPolygonPoints().ToArray().AsSpan();
        var elements = new List<GdsElement>();
        var currentIndex = 0;
        while (currentIndex < allPoints.Length / 2)
        {
            // Check how much we can fit in this element
            var numPoints = Math.Min(maxVertices, (allPoints.Length/2 - currentIndex)*2) / 2;
            numPoints = Math.Max(2, numPoints);
            
            // Get the points from the up and down leg of the polygon
            var pointsUp = allPoints.Slice(currentIndex, numPoints);
            var pointsDown = allPoints.Slice(allPoints.Length - currentIndex - numPoints, numPoints);

            var combined = new List<GdsPoint>(numPoints * 2);
            combined.AddRange(pointsUp);
            combined.AddRange(pointsDown);
            
            elements.Add(new GdsElement
            {
                Element = new GdsBoundaryElement
                {
                    Points = combined
                }
            });

            // Make sure we start at the last point of the previous element
            currentIndex += numPoints-1;
        }

        return elements;
    }
}