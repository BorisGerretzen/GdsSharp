namespace GdsSharp.Lib.Parsing.Tokens;

public class GdsRecordRefLibs : IGdsRecord
{
    public List<string> Libraries { get; set; } = new();

    public void Read(GdsBinaryReader reader, GdsHeader header)
    {
        var numStrings = header.NumToRead / 44;
        for (var i = 0; i < numStrings; i++)
        {
            Libraries.Add(reader.ReadAsciiString(44));
        }
    }
}