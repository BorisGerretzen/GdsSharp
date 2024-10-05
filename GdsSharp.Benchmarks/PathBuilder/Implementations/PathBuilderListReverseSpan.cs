using System.Numerics;
using GdsSharp.Lib.NonTerminals;

namespace GdsSharp.Benchmarks.PathBuilder.Implementations;

public class PathBuilderListReverseSpan : PathBuilderBase
{
    public PathBuilderListReverseSpan(float initialWidth, Vector2? initialPosition = null, Vector2? initialHeading = null) : base(initialWidth, initialPosition, initialHeading)
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

        GdsPoint[] meshUp = new GdsPoint[totalSegments * pointsPerSegment];
        GdsPoint[] meshDown = new GdsPoint[totalSegments * pointsPerSegment];
        int upIndex = 0;
        int downIndex = 0;

        foreach (var (_, points) in segments)
        {
            foreach (var (point, normal, width) in points)
            {
                meshUp[upIndex++] = new GdsPoint(point + normal * width / 2);
                meshDown[downIndex++] = new GdsPoint(point - normal * width / 2);
            }
        }

        // Reverse meshDown in-place using Span<T>
        Span<GdsPoint> meshDownSpan = meshDown.AsSpan(0, downIndex);
        meshDownSpan.Reverse();

        // Combine meshUp and meshDown into a single array
        GdsPoint[] combined = new GdsPoint[upIndex + downIndex];
        Span<GdsPoint> combinedSpan = combined.AsSpan();

        // Copy meshUp
        meshUp.AsSpan(0, upIndex).CopyTo(combinedSpan);

        // Copy reversed meshDown
        meshDownSpan.CopyTo(combinedSpan.Slice(upIndex));

        return combined;
    }
}