using GdsSharp.Lib.Terminals.Abstractions;

namespace GdsSharp.Lib.Terminals.Records;

public class GdsRecordBoxType : GenericGdsRecord<short>
{
    public override ushort Code => 0x2E02;
    
    
}