using GdsSharp.Lib.Parsing.Abstractions;

namespace GdsSharp.Lib.Parsing.Records;

public class GdsRecordPropValue : GenericGdsRecord<string>
{
    public override ushort Code => 0x2C06;
}