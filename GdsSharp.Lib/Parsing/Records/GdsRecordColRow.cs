using GdsSharp.Lib.Parsing.Abstractions;

namespace GdsSharp.Lib.Parsing.Records;

public class GdsRecordColRow : IGdsSimpleRead, IGdsSimpleWrite
{
    public short NumCols { get; set; }
    public short NumRows { get; set; }

    public ushort Code => 0x1302;

    public ushort GetLength()
    {
        return 4;
    }
}