using System.Numerics;
using GdsSharp.Lib.NonTerminals;

namespace GdsSharp.Benchmarks.PathBuilder.Implementations;

public class PathBuilderListReverse : PathBuilderBase
{
    public PathBuilderListReverse(float initialWidth, Vector2? initialPosition = null, Vector2? initialHeading = null) : base(initialWidth, initialPosition, initialHeading)
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
            meshUp.Add(new GdsPoint(point + normal * width / 2));
            meshDown.Add(new GdsPoint(point - normal * width / 2));
        }

        meshDown.Reverse();
        meshUp.AddRange(meshDown);
        return meshUp;
    }
}