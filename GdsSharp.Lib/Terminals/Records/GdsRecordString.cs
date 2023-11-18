using GdsSharp.Lib.Terminals.Abstractions;

namespace GdsSharp.Lib.Terminals.Records;

public class GdsRecordString : GenericGdsRecord<string>
{
    public override ushort Code => 0x1906;
}