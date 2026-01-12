using System.Buffers;
using System.Buffers.Binary;
using System.Text;

namespace GdsSharp.Lib.Reading;

public sealed class BufferedGdsReader : IDisposable
{
    private readonly bool _leaveOpen;
    private readonly Stream _stream;

    private byte[] _buffer;

    private bool _disposed;
    private int _len;
    private int _pos;

    // 1. New field to track the underlying stream position manually
    private long _streamPosition;

    public BufferedGdsReader(Stream stream, bool leaveOpen = true, int bufferSize = 64 * 1024)
    {
        _stream = stream ?? throw new ArgumentNullException(nameof(stream));
        if (!stream.CanRead) throw new ArgumentException("Stream must be readable.", nameof(stream));
        if (bufferSize <= 0) throw new ArgumentOutOfRangeException(nameof(bufferSize));

        _leaveOpen = leaveOpen;
        _buffer = ArrayPool<byte>.Shared.Rent(bufferSize);

        // Initialize our tracker.
        // If the stream is not seekable, Position might throw or return 0.
        // We attempt to sync if possible.
        if (_stream.CanSeek)
            try
            {
                _streamPosition = _stream.Position;
            }
            catch
            {
                _streamPosition = 0;
            }
        else
            _streamPosition = 0;
    }

    public long Position => _streamPosition - (_len - _pos);

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

    public string ReadAsciiString(int length)
    {
        if (length < 0) throw new ArgumentOutOfRangeException(nameof(length));
        if (length == 0) return string.Empty;

        if (TryGetContiguousSpan(length, out var s))
        {
            var trimmed = TrimTrailingNulls(s);
            _pos += length;
            return Encoding.ASCII.GetString(trimmed);
        }

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

    public void ReadExactly(Span<byte> destination)
    {
        if (destination.Length == 0) return;

        var remaining = destination.Length;
        var written = 0;

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

    public void Skip(int numBytes)
    {
        if (numBytes < 0) throw new ArgumentOutOfRangeException(nameof(numBytes));
        if (numBytes == 0) return;

        // Consume buffered data first
        var inBuf = Math.Min(numBytes, _len - _pos);
        _pos += inBuf;
        numBytes -= inBuf;

        if (numBytes == 0) return;

        // Skip in underlying stream
        if (_stream.CanSeek)
        {
            // Seek returns the new position, allowing us to sync perfectly
            _streamPosition = _stream.Seek(numBytes, SeekOrigin.Current);
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

            // Update tracker
            _streamPosition += read;
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

    private bool TryGetContiguousSpan(int count, out ReadOnlySpan<byte> span)
    {
        if (_len - _pos >= count)
        {
            span = _buffer.AsSpan(_pos, count);
            return true;
        }

        if (count > _buffer.Length)
        {
            span = default;
            return false;
        }

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
        var read = _stream.Read(_buffer, 0, _buffer.Length);

        // Update tracker
        _streamPosition += read;

        _len = read;
        if (_len == 0) throw new EndOfStreamException();
    }

    private void FillBufferAtleast(int needed)
    {
        if (_len - _pos >= needed) return;

        var read = _stream.Read(_buffer, _len, _buffer.Length - _len);

        // Update tracker
        _streamPosition += read;

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