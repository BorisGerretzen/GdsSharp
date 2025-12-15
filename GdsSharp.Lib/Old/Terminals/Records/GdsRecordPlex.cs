using GdsSharp.Lib.Old.Terminals.Abstractions;

namespace GdsSharp.Lib.Old.Terminals.Records;

public class GdsRecordPlex : GenericGdsRecord<int>
{
    public override ushort Code => 0x2F03;
}