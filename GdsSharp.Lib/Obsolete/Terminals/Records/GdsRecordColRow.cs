using GdsSharp.Lib.Obsolete.Terminals.Abstractions;

namespace GdsSharp.Lib.Obsolete.Terminals.Records;

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