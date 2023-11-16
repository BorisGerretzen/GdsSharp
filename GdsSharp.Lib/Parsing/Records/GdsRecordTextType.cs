using GdsSharp.Lib.Parsing.Abstractions;

namespace GdsSharp.Lib.Parsing.Records;

public class GdsRecordTextType : GenericGdsRecord<short>
{
    public override ushort Code => 0x1602;
}