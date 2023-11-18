using GdsSharp.Lib.Terminals.Abstractions;

namespace GdsSharp.Lib.Terminals.Records;

public class GdsRecordPlex : GenericGdsRecord<int>
{
    public override ushort Code => 0x2F03;
}