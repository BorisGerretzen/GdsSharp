using GdsSharp.Lib.Obsolete.Terminals.Abstractions;

namespace GdsSharp.Lib.Obsolete.Terminals.Records;

public class GdsRecordString : GenericGdsRecord<string>
{
    public override ushort Code => 0x1906;
}