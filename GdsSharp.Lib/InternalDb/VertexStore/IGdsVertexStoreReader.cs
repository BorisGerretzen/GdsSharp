namespace GdsSharp.Lib.InternalDb.VertexStore;

public interface IGdsVertexStoreReader
{
    int Read(long pointIndex, Span<GdsPoint> destination);
}