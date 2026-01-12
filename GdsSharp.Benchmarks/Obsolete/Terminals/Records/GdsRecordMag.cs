using GdsSharp.Benchmarks.Obsolete.Terminals.Abstractions;

namespace GdsSharp.Benchmarks.Obsolete.Terminals.Records;

public class GdsRecordMag : GenericGdsRecord<double>
{
    public override ushort Code => 0x1B05;
}