using GdsSharp.Benchmarks.Obsolete.Terminals.Abstractions;

namespace GdsSharp.Benchmarks.Obsolete.Terminals.Records;

public class GdsRecordPlex : GenericGdsRecord<int>
{
    public override ushort Code => 0x2F03;
}