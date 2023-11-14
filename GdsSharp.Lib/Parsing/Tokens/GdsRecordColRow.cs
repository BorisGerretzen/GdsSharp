namespace GdsSharp.Lib.Parsing.Tokens;

public class GdsRecordColRow : IGdsSimpleRead
{
    public short NumCols { get; set; }
    public short NumRows { get; set; }

    public ushort Code => 0x1302;

    public int GetLength()
    {
        return 4;
    }
}