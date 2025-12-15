namespace GdsSharp.Lib.InternalDb.VertexStore;

public interface IGdsVertexStoreWriter
{
    int Write(ReadOnlySpan<GdsPoint> points);
}