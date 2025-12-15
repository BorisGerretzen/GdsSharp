using GdsSharp.Lib.Old.Terminals.Abstractions;

namespace GdsSharp.Lib.Old.Terminals.Records;

public class GdsRecordMag : GenericGdsRecord<double>
{
    public override ushort Code => 0x1B05;
}