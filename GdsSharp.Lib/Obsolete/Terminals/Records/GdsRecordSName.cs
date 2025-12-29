using GdsSharp.Lib.Obsolete.Terminals.Abstractions;

namespace GdsSharp.Lib.Obsolete.Terminals.Records;

public class GdsRecordSName : GenericGdsRecord<string>
{
    public override ushort Code => 0x1206;
}