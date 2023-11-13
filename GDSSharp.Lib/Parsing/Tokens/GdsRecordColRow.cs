namespace GdsSharp.Lib.Parsing.Tokens;

public class GdsRecordColRow : IGdsSimpleRead
{
    public short NumCols { get; set; }
    public short NumRows { get; set; }
}