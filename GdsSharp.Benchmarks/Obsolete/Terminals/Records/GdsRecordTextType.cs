using GdsSharp.Benchmarks.Obsolete.Terminals.Abstractions;

namespace GdsSharp.Benchmarks.Obsolete.Terminals.Records;

public class GdsRecordTextType : GenericGdsRecord<short>
{
    public override ushort Code => 0x1602;
}