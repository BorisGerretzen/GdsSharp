using GdsSharp.Lib.Old.Terminals.Abstractions;

namespace GdsSharp.Lib.Old.Terminals.Records;

public class GdsRecordDataType : GenericGdsRecord<short>
{
    public override ushort Code => 0x0E02;
}