using GdsSharp.Lib.Old.Terminals.Abstractions;

namespace GdsSharp.Lib.Old.Terminals.Records;

public class GdsRecordAngle : GenericGdsRecord<double>
{
    public override ushort Code => 0x1C05;
}