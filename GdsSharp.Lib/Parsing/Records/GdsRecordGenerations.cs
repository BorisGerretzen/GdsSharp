using GdsSharp.Lib.Parsing.Abstractions;

namespace GdsSharp.Lib.Parsing.Records;

public class GdsRecordGenerations : GenericGdsRecord<short>
{
    public override short Value { get; set; } = 3;
    public override ushort Code => 0x2202;
}