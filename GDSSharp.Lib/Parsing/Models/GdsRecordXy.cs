using GdsSharp.Lib.Models.Parsing;

namespace GdsSharp.Lib.Parsing.Models;

public class GdsRecordXy : IGdsRecord
{
    public List<(short X, short Y)> Coordinates { get; set; } = new();

    public GdsRecordXy(GdsBinaryReader reader, GdsHeader header)
    {
        var numCoordinates = header.NumToRead / 4;
        for (var i = 0; i < numCoordinates; i++)
        {
            Coordinates.Add((reader.ReadInt16(), reader.ReadInt16()));
        }
    }
}