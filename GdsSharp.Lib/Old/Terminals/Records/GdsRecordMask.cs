using GdsSharp.Lib.Old.Terminals.Abstractions;

namespace GdsSharp.Lib.Old.Terminals.Records;

public class GdsRecordMask : GenericGdsRecord<string>
{
    public override ushort Code => 0x3706;
}