using GdsSharp.Lib.Parsing.Abstractions;

namespace GdsSharp.Lib.Parsing.Records;

public class GdsRecordPropAttr : GenericGdsRecord<short>
{
    public override ushort Code => 0x2B02;
}