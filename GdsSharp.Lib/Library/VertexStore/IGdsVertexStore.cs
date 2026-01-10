namespace GdsSharp.Lib.Library.VertexStore;

public interface IGdsVertexStore
{
    int Read(long pointIndex, Span<GdsPoint> destination);
    int Write(ReadOnlySpan<GdsPoint> points);
}