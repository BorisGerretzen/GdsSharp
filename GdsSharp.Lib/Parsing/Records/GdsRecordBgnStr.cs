using GdsSharp.Lib.Parsing.Abstractions;

namespace GdsSharp.Lib.Parsing.Records;

public class GdsRecordBgnStr : IGdsSimpleRead, IGdsSimpleWrite
{
    public short CreationTimeYear { get; set; }
    public short CreationTimeMonth { get; set; }
    public short CreationTimeDay { get; set; }
    public short CreationTimeHour { get; set; }
    public short CreationTimeMinute { get; set; }
    public short CreationTimeSecond { get; set; }

    public short LastModificationTimeYear { get; set; }
    public short LastModificationTimeMonth { get; set; }
    public short LastModificationTimeDay { get; set; }
    public short LastModificationTimeHour { get; set; }
    public short LastModificationTimeMinute { get; set; }
    public short LastModificationTimeSecond { get; set; }

    public ushort Code => 0x0502;

    public ushort GetLength()
    {
        return 24;
    }
}