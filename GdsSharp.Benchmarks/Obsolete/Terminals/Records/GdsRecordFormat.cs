using GdsSharp.Benchmarks.Obsolete.Terminals.Abstractions;

namespace GdsSharp.Benchmarks.Obsolete.Terminals.Records;

public class GdsRecordFormat : GenericGdsRecord<short>
{
    public override ushort Code => 0x3602;
}