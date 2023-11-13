using GdsSharp.Lib.Models.Parsing;

namespace GdsSharp.Lib.Parsing.Models;

public class GdsRecordColRow : IGdsSimpleRead
{
    public short NumCols { get; set; }
    public short NumRows { get; set; }
}