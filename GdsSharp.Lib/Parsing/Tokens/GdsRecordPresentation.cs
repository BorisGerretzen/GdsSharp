namespace GdsSharp.Lib.Parsing.Tokens;

public class GdsRecordPresentation : IGdsReadableRecord
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
}