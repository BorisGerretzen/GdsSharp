using System.Buffers;
using System.Buffers.Binary;
using System.Text;
using GdsSharp.Lib.Old.Terminals;

namespace GdsSharp.Lib.Binary;

public sealed class BufferedGdsReader : IDisposable
{
    private readonly bool _leaveOpen;
    private readonly Stream _stream;

    private byte[] _buffer;

    private bool _disposed;
    private int _len;
    private int _pos;

    public BufferedGdsReader(Stream stream, bool leaveOpen = true, int bufferSize = 64 * 1024)
    {
        _stream = stream ?? throw new ArgumentNullException(nameof(stream));
        if (!stream.CanRead) throw new ArgumentException("Stream must be readable.", nameof(stream));
        if (bufferSize <= 0) throw new ArgumentOutOfRangeException(nameof(bufferSize));

        _leaveOpen = leaveOpen;
        _buffer = ArrayPool<byte>.Shared.Rent(bufferSize);
    }

    public long Position => _stream.Position - (_len - _pos);

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        var buf = _buffer;
        _buffer = [];
        if (buf.Length != 0)
            ArrayPool<byte>.Shared.Return(buf);

        if (!_leaveOpen)
            _stream.Dispose();
    }

    public short ReadInt16()
    {
        return BinaryPrimitives.ReadInt16BigEndian(GetContiguousSpan(2));
    }

    public ushort ReadUInt16()
    {
        return BinaryPrimitives.ReadUInt16BigEndian(GetContiguousSpan(2));
    }

    public int ReadInt32()
    {
        return BinaryPrimitives.ReadInt32BigEndian(GetContiguousSpan(4));
    }

    public double ReadDouble()
    {
        var span = GetContiguousSpan(GdsDouble.Size);
        return new GdsDouble(span).AsDouble();
    }

    /// <summary>
    ///     Reads an ASCII string of exactly <paramref name="length" /> bytes and trims trailing '\0' padding.
    /// </summary>
    public string ReadAsciiString(int length)
    {
        if (length < 0) throw new ArgumentOutOfRangeException(nameof(length));
        if (length == 0) return string.Empty;

        // Try get from buffer
        if (TryGetContiguousSpan(length, out var s))
        {
            var trimmed = TrimTrailingNulls(s);
            _pos += length;
            return Encoding.ASCII.GetString(trimmed);
        }

        // If not buffered rent a temp array.
        var tmp = ArrayPool<byte>.Shared.Rent(length);
        try
        {
            ReadExactly(tmp.AsSpan(0, length));
            var trimmed = TrimTrailingNulls(tmp.AsSpan(0, length));
            return Encoding.ASCII.GetString(trimmed);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(tmp);
        }
    }

    /// <summary>
    ///     Reads exactly length of the <paramref name="destination" /> span.
    ///     Useful for bulk reads.
    /// </summary>
    public void ReadExactly(Span<byte> destination)
    {
        if (destination.Length == 0) return;

        // Consume from current buffer first.
        var remaining = destination.Length;
        var written = 0;

        // Keep filling buffer and copying until done.
        while (remaining > 0)
        {
            if (_pos == _len)
                FillBuffer();

            var toTake = Math.Min(remaining, _len - _pos);
            _buffer.AsSpan(_pos, toTake).CopyTo(destination.Slice(written, toTake));
            _pos += toTake;
            written += toTake;
            remaining -= toTake;
        }
    }

    /// <summary>
    ///     Skips forward by <paramref name="numBytes" /> bytes.
    /// </summary>
    public void Skip(int numBytes)
    {
        if (numBytes < 0) throw new ArgumentOutOfRangeException(nameof(numBytes));
        if (numBytes == 0) return;

        var inBuf = Math.Min(numBytes, _len - _pos);
        _pos += inBuf;
        numBytes -= inBuf;
        if (numBytes == 0) return;

        if (_stream.CanSeek)
        {
            _stream.Seek(numBytes, SeekOrigin.Current);
            _pos = 0;
            _len = 0;
            return;
        }

        Span<byte> scratch = stackalloc byte[4096];
        while (numBytes > 0)
        {
            var toTake = Math.Min(numBytes, scratch.Length);
            var read = _stream.Read(scratch[..toTake]);
            if (read <= 0) throw new EndOfStreamException();
            numBytes -= read;
        }

        _pos = 0;
        _len = 0;
    }

    private ReadOnlySpan<byte> GetContiguousSpan(int count)
    {
        if (count <= 0) throw new ArgumentOutOfRangeException(nameof(count));

        if (!TryGetContiguousSpan(count, out var span))
            throw new InvalidOperationException("Internal: expected contiguous span for primitive read.");

        _pos += count;
        return span;
    }

    /// <summary>
    ///     Tries to get a contiguous span of <paramref name="count" /> bytes from the buffer, filling/compacting as needed.
    /// </summary>
    /// <param name="count">Number of bytes requested.</param>
    /// <param name="span">Returned span if successful.</param>
    /// <returns>False if no span could be read.</returns>
    private bool TryGetContiguousSpan(int count, out ReadOnlySpan<byte> span)
    {
        if (_len - _pos >= count)
        {
            span = _buffer.AsSpan(_pos, count);
            return true;
        }

        // Can only provide span of up to buffer size.
        if (count > _buffer.Length)
        {
            span = default;
            return false;
        }

        // Compact and try again.
        Compact();
        FillBufferAtleast(count);

        if (_len - _pos >= count)
        {
            span = _buffer.AsSpan(_pos, count);
            return true;
        }

        span = default;
        return false;
    }

    private void FillBuffer()
    {
        _pos = 0;
        _len = _stream.Read(_buffer, 0, _buffer.Length);
        if (_len == 0) throw new EndOfStreamException();
    }

    private void FillBufferAtleast(int needed)
    {
        if (_len - _pos >= needed) return;

        var read = _stream.Read(_buffer, _len, _buffer.Length - _len);
        _len += read;
        if (_len - _pos < needed)
            throw new EndOfStreamException();
    }

    private void Compact()
    {
        if (_pos == 0) return;

        var remaining = _len - _pos;
        if (remaining > 0)
            Buffer.BlockCopy(_buffer, _pos, _buffer, 0, remaining);

        _pos = 0;
        _len = remaining;
    }

    private static ReadOnlySpan<byte> TrimTrailingNulls(ReadOnlySpan<byte> bytes)
    {
        var end = bytes.Length;
        while (end > 0 && bytes[end - 1] == 0) end--;
        return bytes[..end];
    }
}