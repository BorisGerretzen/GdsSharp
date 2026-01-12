using GdsSharp.Benchmarks.Obsolete.Terminals.Abstractions;

namespace GdsSharp.Benchmarks.Obsolete.Terminals.Records;

public class GdsRecordPropAttr : GenericGdsRecord<short>
{
    public override ushort Code => 0x2B02;
}