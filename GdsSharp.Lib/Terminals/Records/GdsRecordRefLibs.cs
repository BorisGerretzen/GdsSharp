using GdsSharp.Lib.Binary;
using GdsSharp.Lib.Terminals.Abstractions;

namespace GdsSharp.Lib.Terminals.Records;

public class GdsRecordRefLibs : IGdsReadableRecord, IGdsWriteableRecord
{
    public List<string> Libraries { get; set; } = new();

    public void Read(GdsBinaryReader reader, GdsHeader header)
    {
        var numStrings = header.NumToRead / 44;
        for (var i = 0; i < numStrings; i++) Libraries.Add(reader.ReadAsciiString(44));
    }

    public ushort Code => 0x1F06;

    public ushort GetLength()
    {
        return (ushort)(Libraries.Count * 44);
    }

    public void Write(GdsBinaryWriter writer)
    {
        foreach (var library in Libraries)
        {
            var lengthDiff = 44 - library.Length;
            if (lengthDiff < 0) throw new ArgumentException($"Library name is too long: {library}");
            var paddedString = library + new string('\0', lengthDiff);
            writer.Write(paddedString);
        }
    }
}