using GdsSharp.Lib.Parsing.Abstractions;

namespace GdsSharp.Lib.Parsing.Records;

public class GdsRecordStrName : GenericGdsRecord<string>
{
    public override ushort Code => 0x0606;
}