namespace GdsSharp.Lib.Parsing.Tokens;

public class GdsRecordXy : IGdsReadableRecord
{
    public List<(int X, int Y)> Coordinates { get; set; } = new();

    public void Read(GdsBinaryReader reader, GdsHeader header)
    {
        var numCoordinates = header.NumToRead / 8;
        for (var i = 0; i < numCoordinates; i++) Coordinates.Add((reader.ReadInt32(), reader.ReadInt32()));
    }
}