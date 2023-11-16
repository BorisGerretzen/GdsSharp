using GdsSharp.Lib.Parsing.Abstractions;

namespace GdsSharp.Lib.Parsing.Records;

public class GdsRecordDataType : GenericGdsRecord<short>
{
    public override ushort Code => 0x0E02;
}