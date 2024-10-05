using System.Numerics;
using GdsSharp.Lib.NonTerminals;

namespace GdsSharp.Benchmarks.PathBuilder.Implementations;

public class PathBuilderListReversePrealloc : PathBuilderBase
{
    public PathBuilderListReversePrealloc(float initialWidth, Vector2? initialPosition = null, Vector2? initialHeading = null) : base(initialWidth, initialPosition, initialHeading)
    {
    }
    
    protected override IEnumerable<GdsPoint> GetPolygonPoints()
    {
        var segments = GetPathPoints();

        int totalSegments = segments.Count;
        if (totalSegments == 0)
            return Array.Empty<GdsPoint>();

        int pointsPerSegment = segments[0].Item2.Count;
        int estimatedTotalPoints = totalSegments * pointsPerSegment * 2;

        // Preallocate lists with estimated capacity
        List<GdsPoint> meshUp = new List<GdsPoint>(estimatedTotalPoints / 2);
        List<GdsPoint> meshDown = new List<GdsPoint>(estimatedTotalPoints / 2);

        foreach (var (_, points) in segments)
        {
            foreach (var (point, normal, width) in points)
            {
                meshUp.Add(new GdsPoint(point + normal * width / 2));
                meshDown.Add(new GdsPoint(point - normal * width / 2));
            }
        }

        // Reverse meshDown to maintain correct winding order
        meshDown.Reverse();

        // Combine meshUp and meshDown efficiently
        meshUp.AddRange(meshDown);

        return meshUp;
    }
}