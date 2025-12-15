using GdsSharp.Lib.Old.Terminals.Abstractions;

namespace GdsSharp.Lib.Old.Terminals.Records;

public class GdsRecordNodeType : GenericGdsRecord<short>
{
    public override ushort Code => 0x2A02;
}