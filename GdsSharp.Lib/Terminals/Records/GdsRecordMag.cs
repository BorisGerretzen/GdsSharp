using GdsSharp.Lib.Terminals.Abstractions;

namespace GdsSharp.Lib.Terminals.Records;

public class GdsRecordMag : GenericGdsRecord<double>
{
    public override ushort Code => 0x1B05;
}