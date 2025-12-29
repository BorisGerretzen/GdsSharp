using GdsSharp.Lib.Obsolete.Terminals.Abstractions;

namespace GdsSharp.Lib.Obsolete.Terminals.Records;

public class GdsRecordTextType : GenericGdsRecord<short>
{
    public override ushort Code => 0x1602;
}