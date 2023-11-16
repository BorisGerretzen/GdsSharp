using GdsSharp.Lib.Parsing.Abstractions;

namespace GdsSharp.Lib.Parsing.Records;

public class GdsRecordHeader : GenericGdsRecord<short>
{
    public override ushort Code => 0x0002;
}