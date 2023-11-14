namespace GdsSharp.Lib.Parsing.Tokens;

public class GdsRecordRefLibs : IGdsReadableRecord
{
    public List<string> Libraries { get; set; } = new();

    public void Read(GdsBinaryReader reader, GdsHeader header)
    {
        var numStrings = header.NumToRead / 44;
        for (var i = 0; i < numStrings; i++) Libraries.Add(reader.ReadAsciiString(44));
    }

    public ushort Code => 0x1F06;

    public int GetLength()
    {
        return Libraries.Count * 44;
    }
}