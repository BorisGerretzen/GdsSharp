using GdsSharp.Lib.Reading.Models;

namespace GdsSharp.Lib.InternalDb.BoundingBox;

public static class GdsBoundingBoxExtensions
{
    public static GdsBoundingBox TransformBoundingBox(this GdsBoundingBox box, GdsStransInfo strans, GdsPoint? origin = null)
    {
        if (box.IsEmpty) return box;

        var reflection = strans.Reflection;
        var mag = strans.Magnification ?? 1.0;
        var angle = strans.Angle ?? 0.0;

        var ang = angle * (Math.PI / 180.0);
        var cos = Math.Cos(ang);
        var sin = Math.Sin(ang);

        origin ??= new GdsPoint(0, 0);

        Span<GdsPoint> corners = stackalloc GdsPoint[4]
        {
            box.Min,
            new(box.Min.X, box.Max.Y),
            new(box.Max.X, box.Min.Y),
            box.Max,
        };

        double minX = double.PositiveInfinity, minY = double.PositiveInfinity;
        double maxX = double.NegativeInfinity, maxY = double.NegativeInfinity;

        foreach (var c in corners)
        {
            double x = c.X;
            double y = c.Y;

            // reflect around X axis
            if (reflection) y = -y;

            // magnification
            x *= mag;
            y *= mag;

            // rotation
            var rx = x * cos - y * sin;
            var ry = x * sin + y * cos;

            rx += origin.Value.X;
            ry += origin.Value.Y;

            if (rx < minX) minX = rx;
            if (ry < minY) minY = ry;
            if (rx > maxX) maxX = rx;
            if (ry > maxY) maxY = ry;
        }

        return new GdsBoundingBox(
            new GdsPoint(FloorToIntClamped(minX), FloorToIntClamped(minY)),
            new GdsPoint(CeilToIntClamped(maxX), CeilToIntClamped(maxY))
        );
    }

    private static int FloorToIntClamped(double v)
    {
        if (v <= int.MinValue) return int.MinValue;
        if (v >= int.MaxValue) return int.MaxValue;
        return (int)Math.Floor(v);
    }

    private static int CeilToIntClamped(double v)
    {
        if (v <= int.MinValue) return int.MinValue;
        if (v >= int.MaxValue) return int.MaxValue;
        return (int)Math.Ceiling(v);
    }
}