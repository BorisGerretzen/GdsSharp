using GdsSharp.Lib.Obsolete.Terminals.Abstractions;

namespace GdsSharp.Lib.Obsolete.Terminals.Records;

public class GdsRecordPropValue : GenericGdsRecord<string>
{
    public override ushort Code => 0x2C06;
}