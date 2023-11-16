using GdsSharp.Lib.Parsing.Abstractions;

namespace GdsSharp.Lib.Parsing.Records;

public class GdsRecordFormat : GenericGdsRecord<short>
{
    public override ushort Code => 0x3602;
}