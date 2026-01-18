namespace GdsSharp.Lib.Library.VertexStore;

public interface IGdsVertexStore
{
    /// <summary>
    /// Reads points starting from the given point index into the destination span.
    /// </summary>
    /// <param name="pointIndex">Index to start reading.</param>
    /// <param name="destination">Span to fill with points.</param>
    /// <returns>Number of points read.</returns>
    int Read(long pointIndex, Span<GdsPoint> destination);
    
    /// <summary>
    /// Reads a specified number of points starting from the given point index into the destination span.
    /// </summary>
    /// <param name="pointIndex">Index to start reading.</param>
    /// <param name="pointCount">Number of points to read.</param>
    /// <param name="destination">Span to fill with points.</param>
    /// <returns>Number of points read.</returns>
    int Read(long pointIndex, int pointCount, Span<GdsPoint> destination)
    {
        return Read(pointIndex, destination[..pointCount]);
    }
    
    /// <summary>
    /// Writes points from the given span into the store.
    /// Returns the starting index where the points were written.
    /// </summary>
    /// <param name="points">Points to write.</param>
    /// <returns>Offset in the store.</returns>
    int Write(ReadOnlySpan<GdsPoint> points);
}