namespace GdsSharp.Lib.Library.VertexStore;

public class ChunkedVertexStore : IGdsVertexStore
{
    // 8192 * 8 bytes -> ~64KB per chunk. 
    private const int ChunkSize = 8192;

    private readonly List<GdsPoint[]> _chunks = new();
    private int _currentChunkIndex = -1;
    private int _currentChunkOffset = ChunkSize;
    private long _totalPoints;

    public int Write(ReadOnlySpan<GdsPoint> points)
    {
        var startOffset = _totalPoints;
        var remaining = points.Length;
        var spanOffset = 0;

        while (remaining > 0)
        {
            // Create new chunk if needed
            if (_currentChunkOffset >= ChunkSize)
            {
                _chunks.Add(new GdsPoint[ChunkSize]);
                _currentChunkIndex++;
                _currentChunkOffset = 0;
            }

            var currentChunk = _chunks[_currentChunkIndex];
            var spaceInChunk = ChunkSize - _currentChunkOffset;
            var toWrite = Math.Min(remaining, spaceInChunk);

            points.Slice(spanOffset, toWrite).CopyTo(currentChunk.AsSpan(_currentChunkOffset));

            _currentChunkOffset += toWrite;
            spanOffset += toWrite;
            remaining -= toWrite;
            _totalPoints += toWrite;
        }

        return (int)startOffset;
    }

    public int Read(long pointIndex, Span<GdsPoint> destination)
    {
        if (pointIndex >= _totalPoints) return 0;

        var pointsToRead = (int)Math.Min(destination.Length, _totalPoints - pointIndex);
        var readSoFar = 0;

        var chunkIdx = (int)(pointIndex / ChunkSize);
        var chunkOffset = (int)(pointIndex % ChunkSize);

        while (readSoFar < pointsToRead)
        {
            var currentChunk = _chunks[chunkIdx];
            var availableInChunk = ChunkSize - chunkOffset;
            var toCopy = Math.Min(pointsToRead - readSoFar, availableInChunk);

            currentChunk.AsSpan(chunkOffset, toCopy).CopyTo(destination.Slice(readSoFar, toCopy));

            readSoFar += toCopy;

            // Move to next chunk
            chunkIdx++;
            chunkOffset = 0;
        }

        return readSoFar;
    }
}