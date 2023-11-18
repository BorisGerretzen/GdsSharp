using GdsSharp.Lib.Terminals.Abstractions;

namespace GdsSharp.Lib.Terminals.Records;

public class GdsRecordPresentation : IGdsReadableRecord, IGdsWriteableRecord
{
    public int FontNumber { get; set; }
    public int VerticalPresentation { get; set; }
    public int HorizontalPresentation { get; set; }

    public void Read(GdsBinaryReader reader, GdsHeader header)
    {
        var values = reader.ReadInt16();
        if (header.NumToRead != 2)
            throw new ArgumentException("Invalid number of bytes", nameof(header));

        FontNumber = (values & 0b110000) >> 4;
        VerticalPresentation = (values & 0b1100) >> 2;
        HorizontalPresentation = values & 0b11;
    }

    public ushort Code => 0x1701;

    public ushort GetLength()
    {
        return 2;
    }

    public void Write(GdsBinaryWriter writer)
    {
        ushort packed = 0;
        packed |= (ushort)((FontNumber & 0b11) << 4);
        packed |= (ushort)((VerticalPresentation & 0b11) << 2);
        packed |= (ushort)(HorizontalPresentation & 0b11);

        writer.Write(packed);
    }
}