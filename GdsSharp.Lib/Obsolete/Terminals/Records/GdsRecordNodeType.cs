using GdsSharp.Lib.Obsolete.Terminals.Abstractions;

namespace GdsSharp.Lib.Obsolete.Terminals.Records;

public class GdsRecordNodeType : GenericGdsRecord<short>
{
    public override ushort Code => 0x2A02;
}