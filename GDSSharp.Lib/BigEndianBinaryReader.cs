using System.Buffers.Binary;
using System.Text;

namespace GdsSharp.Lib;

public class BigEndianBinaryReader : BinaryReader
{
    public BigEndianBinaryReader(Stream input) : base(input)
    {
        
    }

    public BigEndianBinaryReader(Stream input, Encoding encoding) : base(input, encoding)
    {
    }

    public BigEndianBinaryReader(Stream input, Encoding encoding, bool leaveOpen) : base(input, encoding, leaveOpen)
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
}