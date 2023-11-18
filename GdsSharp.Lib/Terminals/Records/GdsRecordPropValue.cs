using GdsSharp.Lib.Terminals.Abstractions;

namespace GdsSharp.Lib.Terminals.Records;

public class GdsRecordPropValue : GenericGdsRecord<string>
{
    public override ushort Code => 0x2C06;
}