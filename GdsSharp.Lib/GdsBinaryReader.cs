using System.Buffers.Binary;
using System.Text;
using GdsSharp.Lib.Terminals;

namespace GdsSharp.Lib;

public class GdsBinaryReader : BinaryReader
{
    public GdsBinaryReader(Stream input) : base(input)
    {
    }

    public override short ReadInt16()
    {
        var data = base.ReadInt16();
        return BitConverter.IsLittleEndian ? BinaryPrimitives.ReverseEndianness(data) : data;
    }

    public override int ReadInt32()
    {
        var data = base.ReadInt32();
        return BitConverter.IsLittleEndian ? BinaryPrimitives.ReverseEndianness(data) : data;
    }

    public override long ReadInt64()
    {
        var data = base.ReadInt64();
        return BitConverter.IsLittleEndian ? BinaryPrimitives.ReverseEndianness(data) : data;
    }

    public override ushort ReadUInt16()
    {
        var data = base.ReadUInt16();
        return BitConverter.IsLittleEndian ? BinaryPrimitives.ReverseEndianness(data) : data;
    }

    public override uint ReadUInt32()
    {
        var data = base.ReadUInt32();
        return BitConverter.IsLittleEndian ? BinaryPrimitives.ReverseEndianness(data) : data;
    }

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
        var data = base.ReadBytes(GdsDouble.Size);

        if (BitConverter.IsLittleEndian)
            for (var i = 0; i < data.Length; i++)
                data[i] = BinaryPrimitives.ReverseEndianness(data[i]);

        return new GdsDouble(data.AsSpan()).AsDouble();
    }

    public string ReadAsciiString(int length)
    {
        var data = base.ReadBytes(length);
        var str = Encoding.ASCII.GetString(data);
        while (str.EndsWith('\0')) str = str[..^1]; // Remove trailing nulls
        return str;
    }
}