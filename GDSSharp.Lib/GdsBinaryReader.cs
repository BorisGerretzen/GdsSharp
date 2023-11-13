﻿using System.Buffers.Binary;
using System.Text;
using GdsSharp.Lib.Parsing;

namespace GdsSharp.Lib;

public class GdsBinaryReader : BinaryReader
{
    public GdsBinaryReader(Stream input) : base(input)
    {
        
    }

    public GdsBinaryReader(Stream input, Encoding encoding) : base(input, encoding)
    {
    }

    public GdsBinaryReader(Stream input, Encoding encoding, bool leaveOpen) : base(input, encoding, leaveOpen)
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
        throw new NotImplementedException();
    }

    public override double ReadDouble()
    {
        var data = base.ReadBytes(GdsDouble.Size);
        return new GdsDouble(data.AsSpan()).AsDouble();
    }

    public string ReadAsciiString(int length)
    {
        var data = base.ReadBytes(length);
        return Encoding.ASCII.GetString(data);
    }
}