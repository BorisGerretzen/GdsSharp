using GdsSharp.Lib.Old.Terminals.Abstractions;

namespace GdsSharp.Lib.Old.Terminals.Records;

public class GdsRecordPathType : GenericGdsRecord<short>
{
    public override short Value { get; set; } = 0;
    public override ushort Code => 0x2102;
}