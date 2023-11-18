using GdsSharp.Lib.Abstractions;

namespace GdsSharp.Lib.NonTerminals.Elements;

public class GdsBoundaryElement : IGdsLayeredElement
{
    public short DataType { get; set; }
    public IList<GdsPoint> Points { get; set; } = new List<GdsPoint>();
    public bool ExternalData { get; set; }
    public bool TemplateData { get; set; }
    public int PlexNumber { get; set; }
    public short Layer { get; set; }
}