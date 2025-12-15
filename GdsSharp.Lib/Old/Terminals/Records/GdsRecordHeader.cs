using GdsSharp.Lib.Old.Terminals.Abstractions;

namespace GdsSharp.Lib.Old.Terminals.Records;

public class GdsRecordHeader : GenericGdsRecord<short>
{
    public override ushort Code => 0x0002;
}