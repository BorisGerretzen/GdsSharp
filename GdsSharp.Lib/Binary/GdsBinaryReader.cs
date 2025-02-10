using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Text;
using GdsSharp.Lib.Terminals;

namespace GdsSharp.Lib.Binary;

public sealed class GdsBinaryReader : BinaryReader
{
    public GdsBinaryReader(Stream input) : base(input, Encoding.UTF8, true)
    {
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override short ReadInt16() => BinaryPrimitives.ReadInt16BigEndian(ReadBytes(2));
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override int ReadInt32() => BinaryPrimitives.ReadInt32BigEndian(ReadBytes(4));
    public override long ReadInt64()
    {
        var data = base.ReadInt64();
        return BitConverter.IsLittleEndian ? BinaryPrimitives.ReverseEndianness(data) : data;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override ushort ReadUInt16() => BinaryPrimitives.ReadUInt16BigEndian(ReadBytes(2));
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override uint ReadUInt32() => BinaryPrimitives.ReadUInt32BigEndian(ReadBytes(4));
    
    public override ulong ReadUInt64()
    {
        var data = base.ReadUInt64();
        return BitConverter.IsLittleEndian ? BinaryPrimitives.ReverseEndianness(data) : data;
    }

    public override float ReadSingle()
    {
        throw new InvalidOperationException("4 byte floats are not supported.");
    }

    public override double ReadDouble()
    {
        var data = ReadBytes(GdsDouble.Size);

        if (BitConverter.IsLittleEndian)
            for (var i = 0; i < data.Length; i++)
                data[i] = BinaryPrimitives.ReverseEndianness(data[i]);

        return new GdsDouble(data.AsSpan()).AsDouble();
    }

    public string ReadAsciiString(int length)
    {
        Span<byte> buffer = stackalloc byte[length];
        var read = Read(buffer);
        if (read != length)
            throw new EndOfStreamException($"Expected {length} bytes, but only read {read} bytes.");
        var nullIndex = buffer.IndexOf((byte)0);
        return Encoding.ASCII.GetString(nullIndex == -1 ? buffer : buffer[..nullIndex]);
    }

    protected override void Dispose(bool disposing)
    {
        // Do nothing, we don't want to close the stream
        // Somehow setting leaveOpen to true does not work.
    }
}