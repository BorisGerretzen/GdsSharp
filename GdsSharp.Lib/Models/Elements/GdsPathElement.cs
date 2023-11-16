using GdsSharp.Lib.Abstractions;
using GdsSharp.Lib.Models.Enum;

namespace GdsSharp.Lib.Models.Elements;

public class GdsPathElement : IGdsLayeredElement
{
    public short DataType { get; set; }
    public GdsPathType PathType { get; set; } = GdsPathType.Square;
    public int Width { get; set; }
    public bool IsAbsoluteWidth { get; set; }
    public IList<GdsPoint> Points { get; set; } = new List<GdsPoint>();
    public bool ExternalData { get; set; }
    public bool TemplateData { get; set; }
    public short PlexNumber { get; set; }
    public short Layer { get; set; }
}