namespace GdsSharp.Lib;

public readonly struct GdsBoundingBox
{
    public GdsPoint Min { get; }
    public GdsPoint Max { get; }

    public bool IsEmpty => Min is { X: 0, Y: 0 } && Max is { X: 0, Y: 0 };

    public GdsBoundingBox(GdsPoint min, GdsPoint max)
    {
        Min = min;
        Max = max;
    }

    public GdsBoundingBox(IEnumerable<GdsPoint> points)
    {
        if (!points.Any()) return;

        Min = new GdsPoint(int.MaxValue, int.MaxValue);
        Max = new GdsPoint(int.MinValue, int.MinValue);

        foreach (var point in points)
        {
            Min = new GdsPoint(Math.Min(Min.X, point.X), Math.Min(Min.Y, point.Y));
            Max = new GdsPoint(Math.Max(Max.X, point.X), Math.Max(Max.Y, point.Y));
        }
    }

    public GdsBoundingBox(IEnumerable<GdsBoundingBox> boundingBoxes)
    {
        if (!boundingBoxes.Any()) return;

        Min = new GdsPoint(int.MaxValue, int.MaxValue);
        Max = new GdsPoint(int.MinValue, int.MinValue);

        foreach (var boundingBox in boundingBoxes)
        {
            Min = new GdsPoint(Math.Min(Min.X, boundingBox.Min.X), Math.Min(Min.Y, boundingBox.Min.Y));
            Max = new GdsPoint(Math.Max(Max.X, boundingBox.Max.X), Math.Max(Max.Y, boundingBox.Max.Y));
        }
    }

    public static GdsBoundingBox operator *(GdsBoundingBox a, double b)
    {
        return new GdsBoundingBox(new GdsPoint(a.Min.X * b, a.Min.Y * b), new GdsPoint(a.Max.X * b, a.Max.Y * b));
    }
}