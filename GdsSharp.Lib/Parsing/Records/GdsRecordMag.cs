using GdsSharp.Lib.Parsing.Abstractions;

namespace GdsSharp.Lib.Parsing.Records;

public class GdsRecordMag : GenericGdsRecord<double>
{
    public override ushort Code => 0x1B05;
}