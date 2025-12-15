using GdsSharp.Lib.Old.Terminals.Abstractions;

namespace GdsSharp.Lib.Old.Terminals.Records;

public class GdsRecordFormat : GenericGdsRecord<short>
{
    public override ushort Code => 0x3602;
}