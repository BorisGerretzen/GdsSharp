using GdsSharp.Lib.Builders;
using GdsSharp.Lib.NonTerminals.Elements;

namespace GdsSharp.Benchmarks.PathBuilder;

public interface IPathBuilder
{
    public IPathBuilder Straight(int length, float? widthStart = null, float? widthEnd = null, Func<float, float?>? width = null, int vertices = 2);
    public IPathBuilder BendDeg(float angle, int radius, Func<float, float?>? width = null, int vertices = 128);
    public IPathBuilder Bezier(Action<BezierBuilder> build, Func<float, float?>? width = null, int vertices = 128);
    public IEnumerable<GdsElement> Build(int maxVertices = 200);
}