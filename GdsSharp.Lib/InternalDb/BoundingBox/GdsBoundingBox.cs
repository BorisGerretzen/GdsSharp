namespace GdsSharp.Lib.InternalDb;

public readonly struct GdsBoundingBox
{
    public GdsPoint Min { get; }
    public GdsPoint Max { get; }

    public bool IsEmpty => Min.X > Max.X || Min.Y > Max.Y;

    public static GdsBoundingBox Empty => new(
        new GdsPoint(int.MaxValue, int.MaxValue),
        new GdsPoint(int.MinValue, int.MinValue)
    );

    public GdsBoundingBox(GdsPoint a, GdsPoint b)
    {
        var minX = Math.Min(a.X, b.X);
        var minY = Math.Min(a.Y, b.Y);
        var maxX = Math.Max(a.X, b.X);
        var maxY = Math.Max(a.Y, b.Y);

        Min = new GdsPoint(minX, minY);
        Max = new GdsPoint(maxX, maxY);
    }
    
    public GdsBoundingBox Union(GdsBoundingBox other)
    {
        if (IsEmpty) return other;
        if (other.IsEmpty) return this;

        var minX = Math.Min(Min.X, other.Min.X);
        var minY = Math.Min(Min.Y, other.Min.Y);
        var maxX = Math.Max(Max.X, other.Max.X);
        var maxY = Math.Max(Max.Y, other.Max.Y);

        return new GdsBoundingBox(new GdsPoint(minX, minY), new GdsPoint(maxX, maxY));
    }

    public static GdsBoundingBox FromPoints(ReadOnlySpan<GdsPoint> points)
    {
        if (points.Length == 0)
            return Empty;
        
        var minX = points[0].X; 
        var minY = points[0].Y;
        var maxX = points[0].X;
        var maxY = points[0].Y;

        foreach (var p in points)
        {
            if (p.X < minX) minX = p.X;
            if (p.Y < minY) minY = p.Y;
            if (p.X > maxX) maxX = p.X;
            if (p.Y > maxY) maxY = p.Y;
        }

        return new GdsBoundingBox(new GdsPoint(minX, minY), new GdsPoint(maxX, maxY));
    }

    public static GdsBoundingBox operator +(GdsBoundingBox a, GdsPoint b)
    {
        return new GdsBoundingBox(
            new GdsPoint(a.Min.X + b.X, a.Min.Y + b.Y),
            new GdsPoint(a.Max.X + b.X, a.Max.Y + b.Y)
        );
    }
}