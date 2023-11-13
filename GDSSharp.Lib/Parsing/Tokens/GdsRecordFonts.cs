namespace GdsSharp.Lib.Parsing.Tokens;

public class GdsRecordFonts : IGdsReadableRecord
{
    public List<string> Fonts { get; set; } = new();

    public void Read(GdsBinaryReader reader, GdsHeader header)
    {
        var numStrings = header.NumToRead / 44;
        for (var i = 0; i < numStrings; i++) Fonts.Add(reader.ReadAsciiString(44));
    }
}