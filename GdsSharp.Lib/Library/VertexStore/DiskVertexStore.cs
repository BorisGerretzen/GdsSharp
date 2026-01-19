using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace GdsSharp.Lib.Library.VertexStore;

public class DiskVertexStore : IGdsVertexStore, IDisposable
{
    private readonly object _lock = new();
    private readonly int _pointSize;
    private readonly FileStream _stream;

    public DiskVertexStore()
    {
        _pointSize = Unsafe.SizeOf<GdsPoint>();

        var tempPath = Path.GetTempFileName();
        _stream = new FileStream(tempPath,
            FileMode.Open,
            FileAccess.ReadWrite,
            FileShare.None,
            65536,
            FileOptions.DeleteOnClose);
    }

    public void Dispose()
    {
        _stream?.Dispose();
    }

    /// <inheritdoc />
    public int Write(ReadOnlySpan<GdsPoint> points)
    {
        lock (_lock)
        {
            var currentByteLength = _stream.Length;
            var startIndex = currentByteLength / _pointSize;

            if (startIndex > int.MaxValue)
                throw new InvalidOperationException("Store has exceeded the maximum capacity addressable by an int index (approx 16GB). Change interface return type to long.");

            _stream.Seek(0, SeekOrigin.End);

            var byteSpan = MemoryMarshal.AsBytes(points);
            _stream.Write(byteSpan);

            return (int)startIndex;
        }
    }

    /// <inheritdoc />
    public int Read(long pointIndex, Span<GdsPoint> destination)
    {
        lock (_lock)
        {
            var byteOffset = pointIndex * _pointSize;

            if (byteOffset >= _stream.Length) return 0;

            _stream.Seek(byteOffset, SeekOrigin.Begin);

            var destBytes = MemoryMarshal.AsBytes(destination);
            var bytesRead = _stream.Read(destBytes);

            return bytesRead / _pointSize;
        }
    }
}