using GdsSharp.Lib.Parsing.Abstractions;

namespace GdsSharp.Lib.Parsing.Records;

public class GdsRecordNodeType : GenericGdsRecord<short>
{
    public override ushort Code => 0x2A02;
}