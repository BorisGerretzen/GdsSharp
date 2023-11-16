using GdsSharp.Lib.Parsing.Abstractions;

namespace GdsSharp.Lib.Parsing.Records;

public class GdsRecordAngle : GenericGdsRecord<double>
{
    public override ushort Code => 0x1C05;
}