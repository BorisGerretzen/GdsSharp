using GdsSharp.Lib.Terminals.Abstractions;

namespace GdsSharp.Lib.Terminals.Records;

public class GdsRecordNodeType : GenericGdsRecord<short>
{
    public override ushort Code => 0x2A02;
}