namespace GdsSharp.Lib.InternalDb.BoundingBox;

public static class GdsBoundingBoxExtensions
{
    public static GdsBoundingBox TransformBoundingBox(in this GdsTransform t, in GdsBoundingBox box)
    {
        if (box.IsEmpty) return box;

        var reflection = false;
        var mag = 1d;
        var angle = 0d;
        if (t.Strans.HasValue)
        {
            reflection = t.Strans.Value.Reflection;
            mag = t.Strans.Value.Magnification ?? 1;
            angle = t.Strans.Value.Angle ?? 0;
        }

        var ang = angle * (Math.PI / 180.0);
        var cos = Math.Cos(ang);
        var sin = Math.Sin(ang);

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

            // translation
            rx += t.Origin.X;
            ry += t.Origin.Y;

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