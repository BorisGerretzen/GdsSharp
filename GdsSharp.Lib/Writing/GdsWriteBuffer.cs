using System.Buffers;
using System.Buffers.Binary;

namespace GdsSharp.Lib.Writing;

public sealed class GdsWriteBuffer(int initialCapacity = ushort.MaxValue)
{
    private byte[] _buffer = new byte[initialCapacity];
    private int _pos;

    public int Length => _pos;

    public Span<byte> WrittenSpanMutable => _buffer.AsSpan(0, _pos);

    public void Clear() => _pos = 0;

    public void Ensure(int additionalBytes)
    {
        var needed = _pos + additionalBytes;
        if (needed <= _buffer.Length) return;

        var newBuf = ArrayPool<byte>.Shared.Rent(Math.Max(needed, _buffer.Length * 2));
        _buffer.AsSpan(0, _pos).CopyTo(newBuf);
        ArrayPool<byte>.Shared.Return(_buffer);
        _buffer = newBuf;
    }

    public void WriteByte(byte v)
    {
        Ensure(1);
        _buffer[_pos++] = v;
    }

    public void WriteUshort(ushort v)
    {
        Ensure(2);
        BinaryPrimitives.WriteUInt16BigEndian(_buffer.AsSpan(_pos, 2), v);
        _pos += 2;
    }

    public void WriteInt(int v)
    {
        Ensure(4);
        BinaryPrimitives.WriteInt32BigEndian(_buffer.AsSpan(_pos, 4), v);
        _pos += 4;
    }

    public void WriteBytes(ReadOnlySpan<byte> src)
    {
        Ensure(src.Length);
        src.CopyTo(_buffer.AsSpan(_pos, src.Length));
        _pos += src.Length;
    }

    public Span<byte> GetSpan(int length)
    {
        Ensure(length);
        var span = _buffer.AsSpan(_pos, length);
        _pos += length;
        return span;
    }
}