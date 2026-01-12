using System.Numerics;
using System.Runtime.InteropServices;

namespace GdsSharp.Lib.Library.BoundingBox;

public readonly struct GdsBoundingBox(GdsPoint min, GdsPoint max)
{
    public readonly GdsPoint Min = min;
    public readonly GdsPoint Max = max;

    public bool IsEmpty => Min.X > Max.X || Min.Y > Max.Y;

    public static readonly GdsBoundingBox Empty = new(
        new GdsPoint(int.MaxValue, int.MaxValue),
        new GdsPoint(int.MinValue, int.MinValue)
    );

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


    public static GdsBoundingBox FromPoints(ReadOnlySpan<GdsPoint> points)
    {
        if (points.Length == 0)
            return Empty;

        // 1. Reinterpret the span of points as a flat span of integers.
        // Memory layout becomes: [X1, Y1, X2, Y2, X3, Y3...]
        // Note: Assuming GdsPoint is struct { int X; int Y; }
        var rawValues = MemoryMarshal.Cast<GdsPoint, int>(points);

        var minX = int.MaxValue;
        var minY = int.MaxValue;
        var maxX = int.MinValue;
        var maxY = int.MinValue;

        var i = 0;

        // 2. SIMD Loop
        // Process chunks of integers at once (e.g., 4 points / 8 ints on AVX2)
        if (Vector.IsHardwareAccelerated && rawValues.Length >= Vector<int>.Count)
        {
            var vMin = new Vector<int>(int.MaxValue);
            var vMax = new Vector<int>(int.MinValue);

            // Loop until we can't fill a full vector
            for (; i <= rawValues.Length - Vector<int>.Count; i += Vector<int>.Count)
            {
                var v = new Vector<int>(rawValues.Slice(i));
                vMin = Vector.Min(vMin, v);
                vMax = Vector.Max(vMax, v);
            }

            // 3. Reduce the vectors
            // vMin now contains [minX_sub1, minY_sub1, minX_sub2, minY_sub2...]
            // We separate the even lanes (X) and odd lanes (Y).
            for (var j = 0; j < Vector<int>.Count; j += 2)
            {
                minX = Math.Min(minX, vMin[j]);
                minY = Math.Min(minY, vMin[j + 1]);

                maxX = Math.Max(maxX, vMax[j]);
                maxY = Math.Max(maxY, vMax[j + 1]);
            }
        }

        // 4. Scalar Tail Loop
        // Process any remaining integers that didn't fit in the vector
        for (; i < rawValues.Length; i += 2)
        {
            var x = rawValues[i];
            var y = rawValues[i + 1];

            if (x < minX) minX = x;
            if (x > maxX) maxX = x;

            if (y < minY) minY = y;
            if (y > maxY) maxY = y;
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