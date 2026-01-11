using GdsSharp.Lib.Obsolete.Terminals.Abstractions;

namespace GdsSharp.Lib.Obsolete.Terminals.Records;

public class GdsRecordGenerations : GenericGdsRecord<short>
{
    public override short Value { get; set; } = 3;
    public override ushort Code => 0x2202;
}