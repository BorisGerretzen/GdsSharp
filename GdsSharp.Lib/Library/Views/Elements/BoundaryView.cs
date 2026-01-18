using System.Buffers;

namespace GdsSharp.Lib.Library.Views.Elements;

public readonly struct BoundaryView
{
    private readonly GdsLibrary _library;
    private readonly int _boundaryIndex;
    
    public readonly int ElementIndex;
    public short Layer => _library.Boundaries[_boundaryIndex].Layer;
    public short DataType => _library.Boundaries[_boundaryIndex].DataType;
    public int PointCount => _library.Boundaries[_boundaryIndex].VertexCount;
    
    internal BoundaryView(GdsLibrary library, int elementIndex)
    {
        _library = library;
        ElementIndex = elementIndex;
        _boundaryIndex = library.Elements[elementIndex].Index;
    }
    
    public int CopyPoints(Span<GdsPoint> buffer)
    {
        if(buffer.Length < PointCount)
            throw new ArgumentException($"Buffer size ({buffer.Length}) is smaller than point count ({PointCount})", nameof(buffer));
        
        var boundary = _library.Boundaries[_boundaryIndex];
        return _library.VertexStore.Read(boundary.VertexOffset, buffer[..PointCount]);
    }
    
    public PooledArray<GdsPoint> GetPoints()
    {
        var arr = ArrayPool<GdsPoint>.Shared.Rent(PointCount);
        CopyPoints(arr);
        return new PooledArray<GdsPoint>(arr, PointCount);
    }
}