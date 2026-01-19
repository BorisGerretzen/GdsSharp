using GdsSharp.Lib.Library.VertexStore;

namespace GdsSharp.Lib.Test.Helpers;

/// <summary>
///     Creates a new memory vertex store for storing GDS points in memory.
///     Pick a suitable initial capacity to avoid resizing.
/// </summary>
/// <param name="initialCapacity">The initial capacity of the vertex store.</param>
public class ReferenceVertexStore(int? initialCapacity = null) : IGdsVertexStore
{
    private readonly List<GdsPoint> _points = new(initialCapacity ?? 0);

    public long Count { get; private set; }

    public int Read(long pointIndex, Span<GdsPoint> destination)
    {
        var availablePoints = _points.Count - pointIndex;
        var pointsToRead = Math.Min(availablePoints, destination.Length);
        if (pointsToRead <= 0)
            return 0;

        for (var i = 0; i < pointsToRead; i++) destination[i] = _points[(int)(pointIndex + i)];

        return (int)pointsToRead;
    }

    public int Write(ReadOnlySpan<GdsPoint> points)
    {
        var offset = _points.Count;
        _points.AddRange(points);
        
        Count += _points.Count;
        return offset;
    }
}