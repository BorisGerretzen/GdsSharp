namespace GdsSharp.Lib.Parsing.Tokens;

public class GdsRecordElFlags : IGdsReadableRecord, IGdsWriteableRecord
{
    public bool ExternalData { get; set; }
    public bool TemplateData { get; set; }

    public void Read(GdsBinaryReader reader, GdsHeader header)
    {
        if (header.NumToRead != 2) throw new ArgumentException($"Invalid number of bytes to read for {nameof(GdsRecordElFlags)}: {header.NumToRead}");

        var data = reader.ReadInt16();
        ExternalData = (data & 0b10) != 0;
        TemplateData = (data & 0b1) != 0;
    }

    public ushort Code => 0x2601;

    public ushort GetLength()
    {
        return 2;
    }

    public void Write(GdsBinaryWriter writer)
    {
        ushort packed = 0;
        packed |= (ushort)((ExternalData ? 1 : 0) << 1);
        packed |= (ushort)(TemplateData ? 1 : 0);
        writer.Write(packed);
    }
}