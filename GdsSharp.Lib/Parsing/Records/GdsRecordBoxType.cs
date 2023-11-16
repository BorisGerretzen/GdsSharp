using GdsSharp.Lib.Parsing.Abstractions;

namespace GdsSharp.Lib.Parsing.Records;

public class GdsRecordBoxType : GenericGdsRecord<short>
{
    public override ushort Code => 0x2E02;
}