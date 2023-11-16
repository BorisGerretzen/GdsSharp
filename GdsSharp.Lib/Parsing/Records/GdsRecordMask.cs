using GdsSharp.Lib.Parsing.Abstractions;

namespace GdsSharp.Lib.Parsing.Records;

public class GdsRecordMask : GenericGdsRecord<string>
{
    public override ushort Code => 0x3706;
}