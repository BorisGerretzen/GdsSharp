using GdsSharp.Lib.Parsing.Abstractions;

namespace GdsSharp.Lib.Parsing.Records;

public class GdsRecordTapeNum : GenericGdsRecord<short>
{
    public override ushort Code => 0x3202;
}