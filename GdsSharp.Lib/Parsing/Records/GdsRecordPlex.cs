using GdsSharp.Lib.Parsing.Abstractions;

namespace GdsSharp.Lib.Parsing.Records;

public class GdsRecordPlex : GenericGdsRecord<int>
{
    public override ushort Code => 0x2F03;
}