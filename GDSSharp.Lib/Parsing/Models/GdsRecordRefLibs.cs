namespace GdsSharp.Lib.Models.Parsing;

public class GdsRecordRefLibs : IGdsRecord
{
    public List<string> Libraries { get; set; } = new();

    /// <summary>
    /// TODO: Untested
    /// </summary>
    /// <param name="reader"></param>
    /// <param name="header"></param>
    internal GdsRecordRefLibs(GdsBinaryReader reader, GdsHeader header)
    {
        var numStrings = header.NumToRead / 44;
        for (var i = 0; i < numStrings; i++)
        {
            Libraries.Add(reader.ReadAsciiString(44));
        }
    }
}