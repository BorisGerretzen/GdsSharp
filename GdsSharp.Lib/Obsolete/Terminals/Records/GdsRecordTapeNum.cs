using GdsSharp.Lib.Obsolete.Terminals.Abstractions;

namespace GdsSharp.Lib.Obsolete.Terminals.Records;

public class GdsRecordTapeNum : GenericGdsRecord<short>
{
    public override ushort Code => 0x3202;
}