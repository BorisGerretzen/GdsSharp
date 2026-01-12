using GdsSharp.Benchmarks.Obsolete.Terminals.Abstractions;

namespace GdsSharp.Benchmarks.Obsolete.Terminals.Records;

public class GdsRecordString : GenericGdsRecord<string>
{
    public override ushort Code => 0x1906;
}