using GdsSharp.Benchmarks.Obsolete.Terminals.Abstractions;

namespace GdsSharp.Benchmarks.Obsolete.Terminals.Records;

public class GdsRecordGenerations : GenericGdsRecord<short>
{
    public override short Value { get; set; } = 3;
    public override ushort Code => 0x2202;
}