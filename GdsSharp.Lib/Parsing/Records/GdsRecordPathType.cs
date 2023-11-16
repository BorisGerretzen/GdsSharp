using GdsSharp.Lib.Parsing.Abstractions;

namespace GdsSharp.Lib.Parsing.Records;

public class GdsRecordPathType : GenericGdsRecord<short>
{
    public override short Value { get; set; } = 0;
    public override ushort Code => 0x2102;
}