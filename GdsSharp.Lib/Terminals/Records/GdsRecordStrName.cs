using GdsSharp.Lib.Terminals.Abstractions;

namespace GdsSharp.Lib.Terminals.Records;

public class GdsRecordStrName : GenericGdsRecord<string>
{
    public override ushort Code => 0x0606;
}