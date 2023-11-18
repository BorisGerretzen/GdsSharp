using GdsSharp.Lib.Terminals.Abstractions;

namespace GdsSharp.Lib.Terminals.Records;

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