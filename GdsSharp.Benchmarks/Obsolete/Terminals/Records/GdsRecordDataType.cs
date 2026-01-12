using GdsSharp.Benchmarks.Obsolete.Terminals.Abstractions;

namespace GdsSharp.Benchmarks.Obsolete.Terminals.Records;

public class GdsRecordDataType : GenericGdsRecord<short>
{
    public override ushort Code => 0x0E02;
}