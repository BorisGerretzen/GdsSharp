using GdsSharp.Lib.NonTerminals.Abstractions;

namespace GdsSharp.Lib.NonTerminals.Elements;

public class GdsBoundaryElement : IGdsLayeredElement
{
    public short DataType { get; set; }
    public IEnumerable<GdsPoint> Points { get; set; } = new List<GdsPoint>();
    public required int NumPoints { get; set; }
    public bool ExternalData { get; set; }
    public bool TemplateData { get; set; }
    public int PlexNumber { get; set; }
    public short Layer { get; set; }

    /// <inheritdoc />
    public void Materialize()
    {
        Points = Points.ToList();
    }

    /// <inheritdoc />
    public GdsBoundingBox GetBoundingBox(GdsStructure.StructureProvider structureProvider)
    {
        return new GdsBoundingBox(Points);
    }
}