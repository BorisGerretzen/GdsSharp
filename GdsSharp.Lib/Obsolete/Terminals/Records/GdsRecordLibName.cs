using GdsSharp.Lib.Obsolete.Terminals.Abstractions;

namespace GdsSharp.Lib.Obsolete.Terminals.Records;

public class GdsRecordLibName : GenericGdsRecord<string>
{
    public override ushort Code => 0x0206;
}