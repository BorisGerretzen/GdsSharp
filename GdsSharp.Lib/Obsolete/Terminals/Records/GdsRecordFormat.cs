using GdsSharp.Lib.Obsolete.Terminals.Abstractions;

namespace GdsSharp.Lib.Obsolete.Terminals.Records;

public class GdsRecordFormat : GenericGdsRecord<short>
{
    public override ushort Code => 0x3602;
}