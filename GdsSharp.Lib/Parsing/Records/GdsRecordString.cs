using GdsSharp.Lib.Parsing.Abstractions;

namespace GdsSharp.Lib.Parsing.Records;

public class GdsRecordString : GenericGdsRecord<string>
{
    public override ushort Code => 0x1906;
}