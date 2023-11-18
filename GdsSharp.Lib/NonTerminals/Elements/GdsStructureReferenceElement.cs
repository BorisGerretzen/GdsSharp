using GdsSharp.Lib.Abstractions;

namespace GdsSharp.Lib.NonTerminals.Elements;

public class GdsStructureReferenceElement : IGdsElement
{
    public required string StructureName { get; set; }
    public GdsStrans Transformation { get; set; } = new();
    public IList<GdsPoint> Points { get; set; } = new List<GdsPoint>();
    public bool ExternalData { get; set; }
    public bool TemplateData { get; set; }
    public int PlexNumber { get; set; }
}