using GdsSharp.Lib.Parsing.Abstractions;

namespace GdsSharp.Lib.Parsing.Records;

public class GdsRecordSTrans : IGdsReadableRecord, IGdsWriteableRecord
{
    public bool Reflection { get; set; }
    public bool AbsoluteMagnification { get; set; }
    public bool AbsoluteAngle { get; set; }

    public void Read(GdsBinaryReader reader, GdsHeader header)
    {
        if (header.NumToRead != 2)
            throw new ArgumentException(
                $"The number of bytes to read for a STrans record should be 2, but was {header.NumToRead}.");

        var data = reader.ReadUInt16();
        Reflection = (data & 0b10000000_00000000) != 0;
        AbsoluteMagnification = (data & 0b100) != 0;
        AbsoluteAngle = (data & 0b10) != 0;
    }

    public ushort Code => 0x1A01;

    public ushort GetLength()
    {
        return 2;
    }

    public void Write(GdsBinaryWriter writer)
    {
        ushort packed = 0;
        packed |= (ushort)((Reflection ? 1 : 0) << 15);
        packed |= (ushort)((AbsoluteMagnification ? 1 : 0) << 2);
        packed |= (ushort)((AbsoluteAngle ? 1 : 0) << 1);
        writer.Write(packed);
    }
}