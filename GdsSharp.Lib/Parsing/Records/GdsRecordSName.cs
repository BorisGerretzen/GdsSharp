using GdsSharp.Lib.Parsing.Abstractions;

namespace GdsSharp.Lib.Parsing.Records;

public class GdsRecordSName : GenericGdsRecord<string>
{
    public override ushort Code => 0x1206;
}