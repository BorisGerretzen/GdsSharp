using GdsSharp.Lib.Terminals.Abstractions;

namespace GdsSharp.Lib.Terminals.Records;

public class GdsRecordAngle : GenericGdsRecord<double>
{
    public override ushort Code => 0x1C05;
}