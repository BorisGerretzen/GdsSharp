using GdsSharp.Lib.Obsolete.Terminals.Abstractions;

namespace GdsSharp.Lib.Obsolete.Terminals.Records;

public class GdsRecordAngle : GenericGdsRecord<double>
{
    public override ushort Code => 0x1C05;
}