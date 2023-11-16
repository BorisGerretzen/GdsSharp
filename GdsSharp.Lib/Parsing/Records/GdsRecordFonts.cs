using GdsSharp.Lib.Parsing.Abstractions;

namespace GdsSharp.Lib.Parsing.Records;

public class GdsRecordFonts : IGdsReadableRecord, IGdsWriteableRecord
{
    public List<string> Fonts { get; set; } = new();

    public void Read(GdsBinaryReader reader, GdsHeader header)
    {
        var numStrings = header.NumToRead / 44;
        for (var i = 0; i < numStrings; i++) Fonts.Add(reader.ReadAsciiString(44));
    }

    public ushort Code => 0x2006;

    public ushort GetLength()
    {
        return (ushort)(Fonts.Count * 44);
    }

    public void Write(GdsBinaryWriter writer)
    {
        foreach (var font in Fonts)
        {
            var lengthDiff = 44 - font.Length;
            if (lengthDiff < 0) throw new ArgumentException($"Font name is too long: {font}");
            var paddedString = font + new string('\0', lengthDiff);
            writer.Write(paddedString);
        }
    }
}