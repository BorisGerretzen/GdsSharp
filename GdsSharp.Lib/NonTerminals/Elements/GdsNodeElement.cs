using GdsSharp.Lib.NonTerminals.Abstractions;

namespace GdsSharp.Lib.NonTerminals.Elements;

public class GdsNodeElement : IGdsLayeredElement
{
    public short NodeType { get; set; }
    public List<GdsPoint> Points { get; set; } = new();
    public bool ExternalData { get; set; }
    public bool TemplateData { get; set; }
    public int PlexNumber { get; set; }
    public short Layer { get; set; }

    /// <inheritdoc />
    public GdsBoundingBox GetBoundingBox(GdsStructure.StructureProvider structureProvider)
    {
        return new GdsBoundingBox(Points);
    }
}