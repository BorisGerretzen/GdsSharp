using GdsSharp.Lib.Terminals.Abstractions;

namespace GdsSharp.Lib.Terminals.Records;

public class GdsRecordSName : GenericGdsRecord<string>
{
    public override ushort Code => 0x1206;
}