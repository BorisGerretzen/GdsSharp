namespace GdsSharp.Lib.InternalDb.VertexStore;

public class MemoryVertexStore : IGdsVertexStoreWriter, IGdsVertexStoreReader
{
    private readonly List<GdsPoint> _points = [];

    public int Write(ReadOnlySpan<GdsPoint> points)
    {
        var offset = _points.Count;
        _points.AddRange(points.ToArray());
        return offset;
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
}