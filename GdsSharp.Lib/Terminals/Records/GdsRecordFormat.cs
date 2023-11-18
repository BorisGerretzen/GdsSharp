using GdsSharp.Lib.Terminals.Abstractions;

namespace GdsSharp.Lib.Terminals.Records;

public class GdsRecordFormat : GenericGdsRecord<short>
{
    public override ushort Code => 0x3602;
}