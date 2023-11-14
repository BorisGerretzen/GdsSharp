namespace GdsSharp.Lib.Parsing.Tokens;

public class GdsRecordBgnLib : IGdsSimpleRead
{
    public short LastModificationTimeYear { get; set; }
    public short LastModificationTimeMonth { get; set; }
    public short LastModificationTimeDay { get; set; }
    public short LastModificationTimeHour { get; set; }
    public short LastModificationTimeMinute { get; set; }
    public short LastModificationTimeSecond { get; set; }

    public short LastAccessTimeYear { get; set; }
    public short LastAccessTimeMonth { get; set; }
    public short LastAccessTimeDay { get; set; }
    public short LastAccessTimeHour { get; set; }
    public short LastAccessTimeMinute { get; set; }
    public short LastAccessTimeSecond { get; set; }

    public ushort Code => 0x0102;

    public int GetLength()
    {
        return 24;
    }
}