using GdsSharp.Lib.Old.Terminals.Abstractions;

namespace GdsSharp.Lib.Old.Terminals.Records;

public class GdsRecordPropAttr : GenericGdsRecord<short>
{
    public override ushort Code => 0x2B02;
}