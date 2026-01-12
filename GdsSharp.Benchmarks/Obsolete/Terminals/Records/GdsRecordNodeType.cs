using GdsSharp.Benchmarks.Obsolete.Terminals.Abstractions;

namespace GdsSharp.Benchmarks.Obsolete.Terminals.Records;

public class GdsRecordNodeType : GenericGdsRecord<short>
{
    public override ushort Code => 0x2A02;
}