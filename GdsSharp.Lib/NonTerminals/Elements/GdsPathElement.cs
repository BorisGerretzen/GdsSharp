using GdsSharp.Lib.Abstractions;
using GdsSharp.Lib.NonTerminals.Enum;

namespace GdsSharp.Lib.NonTerminals.Elements;

public class GdsPathElement : IGdsLayeredElement
{
    public short DataType { get; set; }
    public GdsPathType PathType { get; set; } = GdsPathType.Square;
    public int Width { get; set; }
    public bool IsAbsoluteWidth { get; set; }
    public List<GdsPoint> Points { get; set; } = new();
    public bool ExternalData { get; set; }
    public bool TemplateData { get; set; }
    public int PlexNumber { get; set; }
    public short Layer { get; set; }
}