using GdsSharp.Lib.Old.Terminals.Abstractions;

namespace GdsSharp.Lib.Old.Terminals.Records;

public class GdsRecordLayer : GenericGdsRecord<short>
{
    public override ushort Code => 0x0D02;
}