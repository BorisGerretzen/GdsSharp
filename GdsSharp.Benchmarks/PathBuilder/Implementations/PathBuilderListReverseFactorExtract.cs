using System.Numerics;
using GdsSharp.Lib.NonTerminals;

namespace GdsSharp.Benchmarks.PathBuilder.Implementations;

public class PathBuilderListReverseFactorExtract : PathBuilderBase
{
    public PathBuilderListReverseFactorExtract(float initialWidth, Vector2? initialPosition = null, Vector2? initialHeading = null) : base(initialWidth, initialPosition, initialHeading)
    {
    }
    
    protected override IEnumerable<GdsPoint> GetPolygonPoints()
    {
        var segments = GetPathPoints();

        List<GdsPoint> meshUp = new();
        List<GdsPoint> meshDown = new();

        foreach (var (_, points) in segments)
        foreach (var (point, normal, width) in points)
        {
            var normalWidth = normal * (width / 2f);
            meshUp.Add(new GdsPoint(point + normalWidth));
            meshDown.Add(new GdsPoint(point - normalWidth));
        }

        meshDown.Reverse();
        meshUp.AddRange(meshDown);
        return meshUp;
    }
}