using GdsSharp.Lib.Terminals.Abstractions;

namespace GdsSharp.Lib.Terminals.Records;

public class GdsRecordHeader : GenericGdsRecord<short>
{
    public override ushort Code => 0x0002;
}