using GdsSharp.Lib.Terminals.Abstractions;

namespace GdsSharp.Lib.Terminals.Records;

public class GdsRecordTapeCode : GenericGdsRecord<short>
{
    public override ushort Code => 0x3302;
}