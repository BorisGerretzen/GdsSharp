using GdsSharp.Lib.Old.Terminals.Abstractions;

namespace GdsSharp.Lib.Old.Terminals.Records;

public class GdsRecordPropValue : GenericGdsRecord<string>
{
    public override ushort Code => 0x2C06;
}