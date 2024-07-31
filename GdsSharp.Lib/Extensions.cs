using GdsSharp.Lib.NonTerminals;

namespace GdsSharp.Lib;

public static class Extensions
{
    public static List<GdsPoint> AsGdsPoints(this List<(int x, int y)> coordinates)
    {
        return coordinates.ConvertAll(c => new GdsPoint(c.x, c.y));
    }

    public static List<(int x, int y)> AsTuplePoints(this List<GdsPoint> coordinates)
    {
        return coordinates.ConvertAll(c => (c.X, c.Y));
    }
}