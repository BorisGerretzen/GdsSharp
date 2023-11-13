namespace GdsSharp.Lib.Parsing.Tokens;

public class GdsRecordElFlags : IGdsReadableRecord
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
}