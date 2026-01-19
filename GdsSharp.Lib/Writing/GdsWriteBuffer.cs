using System.Buffers;
using System.Buffers.Binary;

namespace GdsSharp.Lib.Writing;

public sealed class GdsWriteBuffer(int initialCapacity = ushort.MaxValue)
{
    private byte[] _buffer = new byte[initialCapacity];

    public int Length { get; private set; }

    public Span<byte> WrittenSpanMutable => _buffer.AsSpan(0, Length);

    public void Clear()
    {
        Length = 0;
    }

    public void Ensure(int additionalBytes)
    {
        var needed = Length + additionalBytes;
        if (needed <= _buffer.Length) return;

        var newBuf = ArrayPool<byte>.Shared.Rent(Math.Max(needed, _buffer.Length * 2));
        _buffer.AsSpan(0, Length).CopyTo(newBuf);
        ArrayPool<byte>.Shared.Return(_buffer);
        _buffer = newBuf;
    }

    public void WriteByte(byte v)
    {
        Ensure(1);
        _buffer[Length++] = v;
    }

    public void WriteUshort(ushort v)
    {
        Ensure(2);
        BinaryPrimitives.WriteUInt16BigEndian(_buffer.AsSpan(Length, 2), v);
        Length += 2;
    }

    public void WriteInt(int v)
    {
        Ensure(4);
        BinaryPrimitives.WriteInt32BigEndian(_buffer.AsSpan(Length, 4), v);
        Length += 4;
    }

    public void WriteBytes(ReadOnlySpan<byte> src)
    {
        Ensure(src.Length);
        src.CopyTo(_buffer.AsSpan(Length, src.Length));
        Length += src.Length;
    }

    public Span<byte> GetSpan(int length)
    {
        Ensure(length);
        var span = _buffer.AsSpan(Length, length);
        Length += length;
        return span;
    }
}