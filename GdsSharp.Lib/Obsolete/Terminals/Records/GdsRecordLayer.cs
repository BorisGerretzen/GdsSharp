using GdsSharp.Lib.Obsolete.Terminals.Abstractions;

namespace GdsSharp.Lib.Obsolete.Terminals.Records;

public class GdsRecordLayer : GenericGdsRecord<short>
{
    public override ushort Code => 0x0D02;
}