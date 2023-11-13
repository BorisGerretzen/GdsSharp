namespace GdsSharp.Lib.Parsing.Tokens;

public class GdsRecordSTrans : IGdsReadableRecord
{
    public bool Reflection { get; set; }
    public bool AbsoluteMagnification { get; set; }
    public bool AbsoluteAngle { get; set; }

    public void Read(GdsBinaryReader reader, GdsHeader header)
    {
        if (header.NumToRead != 2) throw new ArgumentException($"The number of bytes to read for a STrans record should be 2, but was {header.NumToRead}.");

        var data = reader.ReadUInt16();
        Reflection = (data & 0b10000000_00000000) != 0;
        AbsoluteMagnification = (data & 0b100) != 0;
        AbsoluteAngle = (data & 0b10) != 0;
    }
}