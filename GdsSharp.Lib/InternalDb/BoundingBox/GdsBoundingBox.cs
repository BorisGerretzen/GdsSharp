namespace GdsSharp.Lib.InternalDb.BoundingBox;

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
    
    /// <summary>
    /// Returns the union of this bounding box with another bounding box.
    /// </summary>
    /// <returns>Bounding box that encompasses both this and the other bounding box.</returns>
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

    /// <summary>
    /// Creates a bounding box that encompasses all the given points.
    /// </summary>
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

    /// <summary>
    /// Translates the bounding box by the given vector.
    /// </summary>
    /// <returns>The translated bounding box.</returns>
    public static GdsBoundingBox operator +(GdsBoundingBox box, GdsPoint vec)
    {
        return new GdsBoundingBox(
            new GdsPoint(box.Min.X + vec.X, box.Min.Y + vec.Y),
            new GdsPoint(box.Max.X + vec.X, box.Max.Y + vec.Y)
        );
    }
}