using GdsSharp.Lib.Obsolete.Terminals.Abstractions;

namespace GdsSharp.Lib.Obsolete.Terminals.Records;

public class GdsRecordMag : GenericGdsRecord<double>
{
    public override ushort Code => 0x1B05;
}