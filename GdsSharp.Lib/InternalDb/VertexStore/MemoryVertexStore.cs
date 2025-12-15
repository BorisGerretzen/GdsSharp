using System.Runtime.InteropServices;

namespace GdsSharp.Lib.InternalDb.VertexStore;

public class MemoryVertexStore : IGdsVertexStoreWriter
{
    private readonly List<GdsPoint> _points = [];

    public int Write(ReadOnlySpan<GdsPoint> points)
    {
        _points.AddRange(points.ToArray());
        return points.Length;
    }

    public int Read(long pointIndex, Span<GdsPoint> destination)
    {
        var availablePoints = _points.Count - pointIndex;
        var pointsToRead = Math.Min(availablePoints, destination.Length);
        if (pointsToRead <= 0)
            return 0;

        for (var i = 0; i < pointsToRead; i++)
        {
            destination[i] = _points[(int)(pointIndex + i)];
        }

        return (int)pointsToRead;
    }

    public ReadOnlySpan<GdsPoint> GetSpan(long pointIndex, int length)
    {
        var availablePoints = _points.Count - pointIndex;
        var pointsToGet = Math.Min(availablePoints, length);
        if (pointsToGet <= 0)
            return ReadOnlySpan<GdsPoint>.Empty;

        return CollectionsMarshal.AsSpan(_points).Slice((int)pointIndex, (int)pointsToGet);
    }
}