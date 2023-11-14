namespace GdsSharp.Lib.Parsing.Tokens;

public class GdsRecordHeader : GenericGdsRecord<short>
{
    public override ushort Code => 0x0002;
}