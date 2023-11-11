namespace GdsSharp.Lib.Models.Parsing;

public class GdsHeader : IGdsSimpleRead
{
    public const int RecordSize = 4;

    public ushort Length { get; set; }
    public ushort Code { get; set; }

    public string CodeHex => Code.ToString("X4");
    
    public ushort NumToRead => (ushort)(Length - RecordSize);
}
