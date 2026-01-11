using GdsSharp.Lib.Obsolete.Terminals.Abstractions;

namespace GdsSharp.Lib.Obsolete.Terminals.Records;

public class GdsRecordPlex : GenericGdsRecord<int>
{
    public override ushort Code => 0x2F03;
}