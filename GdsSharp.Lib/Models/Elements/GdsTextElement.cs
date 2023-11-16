using GdsSharp.Lib.Abstractions;
using GdsSharp.Lib.Models.Enum;

namespace GdsSharp.Lib.Models.Elements;

public class GdsTextElement : IGdsLayeredElement
{
    public short TextType { get; set; } = 0;
    public GdsFont Font { get; set; } = GdsFont.Font0;
    public GdsVerticalJustification VerticalJustification { get; set; } = GdsVerticalJustification.Top;
    public GdsHorizontalJustification HorizontalJustification { get; set; } = GdsHorizontalJustification.Left;
    public GdsPathType PathType { get; set; } = GdsPathType.Square;
    public int Width { get; set; }
    public bool IsAbsoluteWidth { get; set; }
    public GdsStrans Transformation { get; set; } = new();
    public IList<GdsPoint> Points { get; set; } = new List<GdsPoint>();
    public required string Text { get; set; }
    public bool ExternalData { get; set; }
    public bool TemplateData { get; set; }
    public short PlexNumber { get; set; }
    public short Layer { get; set; }
}