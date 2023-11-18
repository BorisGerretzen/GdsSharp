using GdsSharp.Lib.Abstractions;

namespace GdsSharp.Lib.NonTerminals.Elements;

public class GdsBoxElement : IGdsLayeredElement
{
    public required short BoxType { get; set; }
    public List<GdsPoint> Points { get; set; } = new List<GdsPoint>();
    public bool ExternalData { get; set; }
    public bool TemplateData { get; set; }
    public int PlexNumber { get; set; }
    public short Layer { get; set; }
}