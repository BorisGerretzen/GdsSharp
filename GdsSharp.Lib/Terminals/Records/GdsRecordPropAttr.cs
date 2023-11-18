using GdsSharp.Lib.Terminals.Abstractions;

namespace GdsSharp.Lib.Terminals.Records;

public class GdsRecordPropAttr : GenericGdsRecord<short>
{
    public override ushort Code => 0x2B02;
}