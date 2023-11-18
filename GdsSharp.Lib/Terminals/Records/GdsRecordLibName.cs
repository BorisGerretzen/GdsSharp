using GdsSharp.Lib.Terminals.Abstractions;

namespace GdsSharp.Lib.Terminals.Records;

public class GdsRecordLibName : GenericGdsRecord<string>
{
    public override ushort Code => 0x0206;
}