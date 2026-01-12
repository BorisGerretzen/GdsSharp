using GdsSharp.Benchmarks.Obsolete.Terminals.Abstractions;

namespace GdsSharp.Benchmarks.Obsolete.Terminals.Records;

public class GdsRecordLayer : GenericGdsRecord<short>
{
    public override ushort Code => 0x0D02;
}