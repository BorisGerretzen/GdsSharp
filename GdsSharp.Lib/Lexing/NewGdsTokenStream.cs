using GdsSharp.Lib.Binary;
using GdsSharp.Lib.Lexing.Tokens;

namespace GdsSharp.Lib.Lexing;

/// <summary>
/// Sequential token reader for GDSII records. No lazy enumerables, no rewind-peek.
/// </summary>
public sealed class NewGdsTokenStream : IDisposable
{
    private readonly Stream _stream;
    private readonly GdsBinaryReader _reader;

    private GdsTokenHeader? _buffered;
    
    private readonly byte[] _buf4 = new byte[4];
    private readonly byte[] _buf8 = new byte[8];

    /// <summary>
    /// Sequential token reader for GDSII records. No lazy enumerables, no rewind-peek.
    /// </summary>
    public NewGdsTokenStream(Stream stream, bool leaveOpen = false)
    {
        _stream = stream ?? throw new ArgumentNullException(nameof(stream));
        _reader = new GdsBinaryReader(stream, leaveOpen);
        if(!_stream.CanSeek) 
            throw new ArgumentException("Stream must support seeking.", nameof(stream));
    }

    /// <summary>
    /// Tries to read a GDS token header from the stream.
    /// </summary>
    /// <param name="header">Read header.</param>
    /// <returns>True if successful.</returns>
    public bool TryRead(out GdsTokenHeader header)
    {
        if (_buffered is not null)
        {
            header = _buffered.Value;
            _buffered = null;
            return true;
        }
        
        return TryReadHeaderInner(out header);
    }

    /// <summary>
    /// Tries to peek a GDS token header from the stream.
    /// </summary>
    /// <param name="header">Peeked header.</param>
    /// <returns>True if successful.</returns>
    public bool TryPeek(out GdsTokenHeader header)
    {
        if (_buffered is not null)
        {
            header = _buffered.Value;
            return true;
        }

        if (!TryReadHeaderInner(out header))
        {
            return false;
        }

        _buffered = header;
        return true;
    }

    /// <summary>
    /// Reads a GDS token header from the stream.
    /// </summary>
    /// <exception cref="EndOfStreamException">If no header could be read.</exception>
    public GdsTokenHeader Read() => TryRead(out var header) ? header : throw new EndOfStreamException();
    
    /// <summary>
    /// Peeks a GDS token header from the stream.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="EndOfStreamException">If no header could be read.</exception>
    public GdsTokenHeader Peek() => TryPeek(out var header) ? header : throw new EndOfStreamException();
    
    #region Payload reading

    public short ReadInt16()
    {
        var value = _reader.ReadInt16();
        return value;
    }
    
    public int ReadInt32()
    {
        var value = _reader.ReadInt32();
        return value;
    }
    
    public long ReadInt64()
    {
        var value = _reader.ReadInt64();
        return value;
    }
    
    public ushort ReadUInt16()
    {
        var value = _reader.ReadUInt16();
        return value;
    }
    
    public uint ReadUInt32()
    {
        var value = _reader.ReadUInt32();
        return value;
    }
    
    public ulong ReadUInt64()
    {
        var value = _reader.ReadUInt64();
        return value;
    }
    
    public double ReadDouble()
    {
        var value = _reader.ReadDouble();
        return value;
    }
    
    public string ReadString(GdsTokenHeader header)
    {
        return _reader.ReadAsciiString(header.PayloadLength);
    }
    
    public string ReadString(int length)
    {
        return _reader.ReadAsciiString(length);
    }

    public void ReadRawBytes(GdsTokenHeader header, Span<byte> destination)
    {
        var length = header.PayloadLength;
        if (destination.Length < length)
            throw new ArgumentException($"Destination span too small for {length} bytes.", nameof(destination));
        var read = _stream.Read(destination[..length]);
        if (read != length)
            throw new EndOfStreamException("Unexpected EOF while reading raw bytes.");
    }
    
    public int ReadXy(GdsTokenHeader header, Span<GdsPoint> destination)
    {
        var payloadLength = header.PayloadLength;
        if ((payloadLength & 7) != 0)
            throw new InvalidDataException($"XY payload bytes ({payloadLength}) not multiple of 8.");
        
        var numPoints = payloadLength / 8;
        if (destination.Length < numPoints)
            throw new ArgumentException($"Destination span too small for {numPoints} points.", nameof(destination));

        for (var i = 0; i < numPoints; i++)
        {
            var x = _reader.ReadInt32();
            var y = _reader.ReadInt32();
            destination[i] = new GdsPoint(x, y);
        }
        
        return numPoints;
    }
    
    public void SkipPayload(GdsTokenHeader header)
    {
        Skip(header.PayloadLength);
    }
    
    #endregion
    
    private bool TryReadHeaderInner(out GdsTokenHeader header)
    {
        var pos = _stream.Position;
        
        var read = _stream.Read(_buf4, 0, 4);
        if (read == 0)
        {
            header = default;
            return false; // EOF
        }
        if (read != 4)
            throw new EndOfStreamException("Unexpected EOF while reading GDS record header.");

        var length = (ushort)((_buf4[0] << 8) | _buf4[1]);
        var code   = (ushort)((_buf4[2] << 8) | _buf4[3]);

        // Padding
        if (length == 0 && code == 0)
        {
            header = default;
            return false;
        }
        
        if (length < 4)
            throw new InvalidDataException($"Invalid record length: {length}");

        header = new GdsTokenHeader(length, code, pos, _stream.Position);
        return true;
    }
    
    private void Skip(int byteCount)
    {
        if (byteCount <= 0) return;
        if (!_stream.CanSeek)
        {
            Span<byte> scratch = stackalloc byte[256];
            var remaining = byteCount;
            while (remaining > 0)
            {
                var take = Math.Min(remaining, scratch.Length);
                var read = _stream.Read(scratch[..take]);
                if (read <= 0) throw new EndOfStreamException();
                remaining -= read;
            }
        }
        else
        {
            _stream.Position += byteCount;
        }
    }

    public void Dispose()
    {
        _reader.Dispose();
    }
}
