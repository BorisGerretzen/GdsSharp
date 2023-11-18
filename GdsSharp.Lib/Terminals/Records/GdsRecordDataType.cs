using GdsSharp.Lib.Terminals.Abstractions;

namespace GdsSharp.Lib.Terminals.Records;

public class GdsRecordDataType : GenericGdsRecord<short>
{
    public override ushort Code => 0x0E02;
}