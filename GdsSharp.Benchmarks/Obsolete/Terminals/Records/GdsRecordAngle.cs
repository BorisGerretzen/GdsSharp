using GdsSharp.Benchmarks.Obsolete.Terminals.Abstractions;

namespace GdsSharp.Benchmarks.Obsolete.Terminals.Records;

public class GdsRecordAngle : GenericGdsRecord<double>
{
    public override ushort Code => 0x1C05;
}