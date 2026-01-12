using GdsSharp.Benchmarks.Obsolete.Terminals.Abstractions;

namespace GdsSharp.Benchmarks.Obsolete.Terminals.Records;

public class GdsRecordPropValue : GenericGdsRecord<string>
{
    public override ushort Code => 0x2C06;
}