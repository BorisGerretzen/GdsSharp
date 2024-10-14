using GdsSharp.Lib.Binary;
using GdsSharp.Lib.NonTerminals;
using GdsSharp.Lib.Terminals.Abstractions;

namespace GdsSharp.Lib.Terminals.Records;

public class GdsRecordXy : IGdsWriteableRecord
{
    public required IEnumerable<GdsPoint> Coordinates { get; set; }
    public required int NumPoints { get; set; }
    
    public GdsRecordXy()
    {
        Coordinates = new List<GdsPoint>();
    }
    
    public ushort Code => 0x1003;

    public ushort GetLength()
    {
        return (ushort)(NumPoints * 8);
    }

    public void Write(GdsBinaryWriter writer)
    {
        foreach (var point in Coordinates)
        {
            writer.Write(point.X);
            writer.Write(point.Y);
        }
    }
}