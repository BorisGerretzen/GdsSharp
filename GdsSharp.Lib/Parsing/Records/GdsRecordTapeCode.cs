using GdsSharp.Lib.Parsing.Abstractions;

namespace GdsSharp.Lib.Parsing.Records;

public class GdsRecordTapeCode : GenericGdsRecord<short>
{
    public override ushort Code => 0x3302;
}