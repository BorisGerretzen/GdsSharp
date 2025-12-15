using GdsSharp.Lib.Old.Terminals.Abstractions;

namespace GdsSharp.Lib.Old.Terminals.Records;

public class GdsRecordStrName : GenericGdsRecord<string>
{
    public override ushort Code => 0x0606;
}