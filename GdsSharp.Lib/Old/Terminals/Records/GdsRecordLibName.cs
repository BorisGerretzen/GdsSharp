using GdsSharp.Lib.Old.Terminals.Abstractions;

namespace GdsSharp.Lib.Old.Terminals.Records;

public class GdsRecordLibName : GenericGdsRecord<string>
{
    public override ushort Code => 0x0206;
}