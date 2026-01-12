using GdsSharp.Lib;

namespace GdsSharp.Benchmarks.Obsolete;

public static class GdsPointExtensions
{
    public static GdsPoint Rotate(this GdsPoint pt, float sin, float cos)
    {
        return new GdsPoint(
            cos * pt.X - sin * pt.Y,
            sin * pt.X + cos * pt.Y
        );
    }
}