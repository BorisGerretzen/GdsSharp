using System.Buffers.Binary;
using System.Runtime.InteropServices;
using GdsSharp.Lib.Binary;

namespace GdsSharp.Lib.Lexing;

public sealed class NewGdsTokenStream : IDisposable
{
    private readonly byte[] _hdr4 = new byte[4];
    private readonly BufferedGdsReader _reader;
    private GdsTokenHeader? _buffered;

    public NewGdsTokenStream(Stream stream, bool leaveOpen = false)
    {
        ArgumentNullException.ThrowIfNull(stream);
        if (!stream.CanSeek)
            throw new ArgumentException("Stream must support seeking.", nameof(stream));

        _reader = new BufferedGdsReader(stream, leaveOpen);
    }

    public void Dispose()
    {
        _reader.Dispose();
    }

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

    public bool TryPeek(out GdsTokenHeader header)
    {
        if (_buffered is not null)
        {
            header = _buffered.Value;
            return true;
        }

        if (!TryReadHeaderInner(out header))
            return false;

        _buffered = header;
        return true;
    }

    public GdsTokenHeader Read()
    {
        return TryRead(out var h) ? h : throw new EndOfStreamException();
    }

    public GdsTokenHeader Peek()
    {
        return TryPeek(out var h) ? h : throw new EndOfStreamException();
    }

    private bool TryReadHeaderInner(out GdsTokenHeader header)
    {
        var pos = _reader.Position;

        try
        {
            _reader.ReadExactly(_hdr4);
        }
        catch (EndOfStreamException)
        {
            header = default;
            return false;
        }

        var length = (ushort)((_hdr4[0] << 8) | _hdr4[1]);
        var code = (ushort)((_hdr4[2] << 8) | _hdr4[3]);

        // Padding
        if (length == 0 && code == 0)
        {
            header = default;
            return false;
        }

        if (length < 4)
            throw new InvalidDataException($"Invalid record length: {length}");

        header = new GdsTokenHeader(length, code, pos, _reader.Position);
        return true;
    }

    #region Payload reading

    public short ReadInt16()
    {
        return _reader.ReadInt16();
    }

    public int ReadInt32()
    {
        return _reader.ReadInt32();
    }

    public ushort ReadUInt16()
    {
        return _reader.ReadUInt16();
    }

    public double ReadDouble()
    {
        return _reader.ReadDouble();
    }

    public string ReadString(GdsTokenHeader header)
    {
        return _reader.ReadAsciiString(header.PayloadLength);
    }

    public string ReadString(int length)
    {
        return _reader.ReadAsciiString(length);
    }

    public int ReadXy(GdsTokenHeader header, Span<GdsPoint> destination)
    {
        var payloadLength = header.PayloadLength;
        if ((payloadLength & 7) != 0)
            throw new InvalidDataException($"XY payload bytes ({payloadLength}) not multiple of 8.");

        var numPoints = payloadLength / 8;
        if (destination.Length < numPoints)
            throw new ArgumentException($"Destination span too small for {numPoints} points.", nameof(destination));

        var bytes = MemoryMarshal.AsBytes(destination[..numPoints]);
        _reader.ReadExactly(bytes);

        var ints = MemoryMarshal.Cast<byte, int>(bytes);
        for (var i = 0; i < ints.Length; i++)
            ints[i] = BinaryPrimitives.ReverseEndianness(ints[i]);

        return numPoints;
    }

    public void SkipPayload(GdsTokenHeader header)
    {
        _reader.Skip(header.PayloadLength);
    }

    #endregion
}