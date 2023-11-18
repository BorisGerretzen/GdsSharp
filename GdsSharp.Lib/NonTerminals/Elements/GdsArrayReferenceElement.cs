using GdsSharp.Lib.Abstractions;

namespace GdsSharp.Lib.NonTerminals.Elements;

public class GdsArrayReferenceElement : IGdsElement
{
    public required string StructureName { get; set; }
    public GdsStrans Transformation { get; set; } = new();
    public required int Rows { get; set; }
    public required int Columns { get; set; }
    public IList<GdsPoint> Points { get; set; } = new List<GdsPoint>();
    public bool ExternalData { get; set; }
    public bool TemplateData { get; set; }
    public int PlexNumber { get; set; }
}