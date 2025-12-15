using GdsSharp.Lib.Old.Terminals.Abstractions;

namespace GdsSharp.Lib.Old.Terminals.Records;

public class GdsRecordTextType : GenericGdsRecord<short>
{
    public override ushort Code => 0x1602;
}