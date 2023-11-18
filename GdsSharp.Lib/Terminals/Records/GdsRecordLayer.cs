using GdsSharp.Lib.Terminals.Abstractions;

namespace GdsSharp.Lib.Terminals.Records;

public class GdsRecordLayer : GenericGdsRecord<short>
{
    public override ushort Code => 0x0D02;
}