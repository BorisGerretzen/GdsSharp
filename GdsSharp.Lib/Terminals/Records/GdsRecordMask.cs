using GdsSharp.Lib.Terminals.Abstractions;

namespace GdsSharp.Lib.Terminals.Records;

public class GdsRecordMask : GenericGdsRecord<string>
{
    public override ushort Code => 0x3706;
}