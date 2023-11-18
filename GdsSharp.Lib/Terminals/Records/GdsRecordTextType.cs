using GdsSharp.Lib.Terminals.Abstractions;

namespace GdsSharp.Lib.Terminals.Records;

public class GdsRecordTextType : GenericGdsRecord<short>
{
    public override ushort Code => 0x1602;
}