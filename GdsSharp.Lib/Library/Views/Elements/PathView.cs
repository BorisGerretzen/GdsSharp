using System.Buffers;
using GdsSharp.Lib.Reading.Enum;

namespace GdsSharp.Lib.Library.Views.Elements;

public readonly struct PathView
{
    private readonly GdsLibrary _library;
    private readonly int _pathIndex;

    public readonly int ElementIndex;
    public short Layer => _library.Paths[_pathIndex].Layer;
    public short DataType => _library.Paths[_pathIndex].DataType;
    public GdsPathType? PathType => _library.Paths[_pathIndex].PathType;
    public int? Width => _library.Paths[_pathIndex].Width;
    public int PointCount => _library.Paths[_pathIndex].VertexCount;

    internal PathView(GdsLibrary library, int elementIndex)
    {
        _library = library;
        ElementIndex = elementIndex;
        _pathIndex = library.Elements[elementIndex].Index;
    }

    public int CopyPoints(Span<GdsPoint> buffer)
    {
        if (buffer.Length < PointCount)
            throw new ArgumentException($"Buffer size ({buffer.Length}) is smaller than point count ({PointCount})", nameof(buffer));

        var path = _library.Paths[_pathIndex];
        return _library.VertexStore.Read(path.VertexOffset, buffer[..PointCount]);
    }

    public PooledArray<GdsPoint> GetPoints()
    {
        var arr = ArrayPool<GdsPoint>.Shared.Rent(PointCount);
        CopyPoints(arr);
        return new PooledArray<GdsPoint>(arr, PointCount);
    }
}