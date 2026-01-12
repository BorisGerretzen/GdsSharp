using GdsSharp.Benchmarks.Obsolete.Binary;
using GdsSharp.Benchmarks.Obsolete.Terminals.Abstractions;
using GdsSharp.Lib;

namespace GdsSharp.Benchmarks.Obsolete.Terminals.Records;

public class GdsRecordXy : IGdsWriteableRecord
{
    public GdsRecordXy()
    {
        Coordinates = new List<GdsPoint>();
    }

    public required IEnumerable<GdsPoint> Coordinates { get; set; }
    public required int NumPoints { get; set; }

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