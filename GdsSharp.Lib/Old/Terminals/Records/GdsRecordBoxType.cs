using GdsSharp.Lib.Old.Terminals.Abstractions;

namespace GdsSharp.Lib.Old.Terminals.Records;

public class GdsRecordBoxType : GenericGdsRecord<short>
{
    public override ushort Code => 0x2E02;
}