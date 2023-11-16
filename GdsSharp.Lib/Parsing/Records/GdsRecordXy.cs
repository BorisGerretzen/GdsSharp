using GdsSharp.Lib.Parsing.Abstractions;

namespace GdsSharp.Lib.Parsing.Records;

public class GdsRecordXy : IGdsReadableRecord, IGdsWriteableRecord
{
    public List<(int X, int Y)> Coordinates { get; set; } = new();

    public void Read(GdsBinaryReader reader, GdsHeader header)
    {
        var numCoordinates = header.NumToRead / 8;
        for (var i = 0; i < numCoordinates; i++) Coordinates.Add((reader.ReadInt32(), reader.ReadInt32()));
    }

    public ushort Code => 0x1003;

    public ushort GetLength()
    {
        return (ushort)(Coordinates.Count * 8);
    }

    public void Write(GdsBinaryWriter writer)
    {
        foreach (var (x, y) in Coordinates)
        {
            writer.Write(x);
            writer.Write(y);
        }
    }
}