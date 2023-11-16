using GdsSharp.Lib.Parsing.Abstractions;

namespace GdsSharp.Lib.Parsing.Records;

public class GdsRecordLibName : GenericGdsRecord<string>
{
    public override ushort Code => 0x0206;
}