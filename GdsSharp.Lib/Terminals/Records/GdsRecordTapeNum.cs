using GdsSharp.Lib.Terminals.Abstractions;

namespace GdsSharp.Lib.Terminals.Records;

public class GdsRecordTapeNum : GenericGdsRecord<short>
{
    public override ushort Code => 0x3202;
}