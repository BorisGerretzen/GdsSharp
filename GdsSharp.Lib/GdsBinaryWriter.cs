using System.Buffers.Binary;
using System.Text;
using GdsSharp.Lib.Parsing;

namespace GdsSharp.Lib;

public class GdsBinaryWriter : BinaryWriter
{
    public GdsBinaryWriter(Stream output) : base(output)
    {
    }

    public override void Write(ushort value)
    {
        var data = BitConverter.IsLittleEndian ? BinaryPrimitives.ReverseEndianness(value) : value;
        base.Write(data);
    }

    public override void Write(uint value)
    {
        var data = BitConverter.IsLittleEndian ? BinaryPrimitives.ReverseEndianness(value) : value;
        base.Write(data);
    }

    public override void Write(ulong value)
    {
        var data = BitConverter.IsLittleEndian ? BinaryPrimitives.ReverseEndianness(value) : value;
        base.Write(data);
    }

    public override void Write(short value)
    {
        var data = BitConverter.IsLittleEndian ? BinaryPrimitives.ReverseEndianness(value) : value;
        base.Write(data);
    }

    public override void Write(int value)
    {
        var data = BitConverter.IsLittleEndian ? BinaryPrimitives.ReverseEndianness(value) : value;
        base.Write(data);
    }

    public override void Write(long value)
    {
        var data = BitConverter.IsLittleEndian ? BinaryPrimitives.ReverseEndianness(value) : value;
        base.Write(data);
    }

    public override void Write(float value)
    {
        throw new NotImplementedException("4 byte floats are not supported.");
    }

    public override void Write(double value)
    {
        var data = new GdsDouble(value);
        base.Write(data.AsBytes());
    }

    public override void Write(string value)
    {
        if (value.Length % 2 != 0)
            value += '\0';

        var data = Encoding.ASCII.GetBytes(value);
        base.Write(data);
    }
}