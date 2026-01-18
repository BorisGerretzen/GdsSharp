using System.Buffers;

namespace GdsSharp.Lib.Library.Views.Elements;

public readonly struct NodeView
{
    private readonly GdsLibrary _library;
    private readonly int _nodeIndex;
    
    public readonly int ElementIndex;
    public short Layer => _library.Nodes[_nodeIndex].Layer;
    public short NodeType => _library.Nodes[_nodeIndex].NodeType;
    public int PointCount => _library.Nodes[_nodeIndex].VertexCount;
    
    internal NodeView(GdsLibrary library, int elementIndex)
    {
        _library = library;
        ElementIndex = elementIndex;
        _nodeIndex = library.Elements[elementIndex].Index;
    }
    
    public int CopyPoints(Span<GdsPoint> buffer)
    {
        if(buffer.Length < PointCount)
            throw new ArgumentException($"Buffer size ({buffer.Length}) is smaller than point count ({PointCount})", nameof(buffer));
        
        var node = _library.Nodes[_nodeIndex];
        return _library.VertexStore.Read(node.VertexOffset, buffer[..PointCount]);
    }
    
    public PooledArray<GdsPoint> GetPoints()
    {
        var arr = ArrayPool<GdsPoint>.Shared.Rent(PointCount);
        CopyPoints(arr);
        return new PooledArray<GdsPoint>(arr, PointCount);
    }
}
