using System.Numerics;
using GdsSharp.Lib.NonTerminals;

namespace GdsSharp.Benchmarks.PathBuilder.Implementations;

public class PathBuilderStack : PathBuilderBase
{
    public PathBuilderStack(float initialWidth, Vector2? initialPosition = null, Vector2? initialHeading = null) : base(initialWidth, initialPosition, initialHeading)
    {
    }

    protected override IEnumerable<GdsPoint> GetPolygonPoints()
    {
        var segments = GetPathPoints();

        List<GdsPoint> meshUp = new();
        Stack<GdsPoint> meshDown = new();

        foreach (var (_, points) in segments)
        foreach (var (point, normal, width) in points)
        {
            meshUp.Add(new GdsPoint(point + normal * width / 2));
            meshDown.Push(new GdsPoint(point - normal * width / 2));
        }

        meshUp.AddRange(meshDown);
        return meshUp;
    }
}