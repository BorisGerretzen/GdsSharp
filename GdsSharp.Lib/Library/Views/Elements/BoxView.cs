using System.Buffers;

namespace GdsSharp.Lib.Library.Views.Elements;

public readonly struct BoxView
{
    private readonly GdsLibrary _library;
    private readonly int _boxIndex;
    
    public readonly int ElementIndex;
    public short Layer => _library.Boxes[_boxIndex].Layer;
    public short BoxType => _library.Boxes[_boxIndex].BoxType;
    
    internal BoxView(GdsLibrary library, int elementIndex)
    {
        _library = library;
        ElementIndex = elementIndex;
        _boxIndex = library.Elements[elementIndex].Index;
    }
    
    public int CopyPoints(Span<GdsPoint> buffer)
    {
        if(buffer.Length < GdsGlobals.BoxPointCount)
            throw new ArgumentException($"Buffer size ({buffer.Length}) is smaller than point count ({GdsGlobals.BoxPointCount})", nameof(buffer));
        
        var box = _library.Boxes[_boxIndex];
        return _library.VertexStore.Read(box.VertexOffset, buffer[..GdsGlobals.BoxPointCount]);
    }
    
    public PooledArray<GdsPoint> GetPoints()
    {
        var arr = ArrayPool<GdsPoint>.Shared.Rent(GdsGlobals.BoxPointCount);
        CopyPoints(arr);
        return new PooledArray<GdsPoint>(arr, GdsGlobals.BoxPointCount);
    }
}
