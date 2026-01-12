using GdsSharp.Benchmarks.Obsolete.Terminals.Abstractions;

namespace GdsSharp.Benchmarks.Obsolete.Terminals.Records;

public class GdsRecordPathType : GenericGdsRecord<short>
{
    public override short Value { get; set; } = 0;
    public override ushort Code => 0x2102;
}