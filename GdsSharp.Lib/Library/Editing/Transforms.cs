namespace GdsSharp.Lib.Library.Editing;

public enum TransformOp
{
    Translate,
    Rotate90Clockwise,
    Rotate90CounterClockwise,
    MirrorX,
    MirrorY
}

internal static class TransformUtil
{
    public static GdsPoint Apply(in GdsPoint p, TransformOp op, in GdsPoint pivot, int dx, int dy)
    {
        return op switch
        {
            TransformOp.Translate => new GdsPoint(p.X + dx, p.Y + dy),
            TransformOp.Rotate90Clockwise => Rotate90Clockwise(p, pivot),
            TransformOp.Rotate90CounterClockwise => Rotate90CounterClockwise(p, pivot),
            TransformOp.MirrorX => new GdsPoint(p.X, pivot.Y + (pivot.Y - p.Y)),
            TransformOp.MirrorY => new GdsPoint(pivot.X + (pivot.X - p.X), p.Y),
            _ => p
        };
    }

    private static GdsPoint Rotate90Clockwise(in GdsPoint p, in GdsPoint pivot)
    {
        var x = p.X - pivot.X;
        var y = p.Y - pivot.Y;
        // (x,y) -> (y, -x)
        return new GdsPoint(pivot.X + y, pivot.Y - x);
    }

    private static GdsPoint Rotate90CounterClockwise(in GdsPoint p, in GdsPoint pivot)
    {
        var x = p.X - pivot.X;
        var y = p.Y - pivot.Y;
        // (x,y) -> (-y, x)
        return new GdsPoint(pivot.X - y, pivot.Y + x);
    }
}