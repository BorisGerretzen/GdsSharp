using GdsSharp.Benchmarks.Obsolete.Terminals.Abstractions;

namespace GdsSharp.Benchmarks.Obsolete.Terminals.Records;

public class GdsRecordBoxType : GenericGdsRecord<short>
{
    public override ushort Code => 0x2E02;
}