using GdsSharp.Lib.NonTerminals.Abstractions;

namespace GdsSharp.Lib.NonTerminals.Elements;

public class GdsStructureReferenceElement : IGdsElement
{
    public required string StructureName { get; set; }
    public GdsStrans Transformation { get; set; } = new();
    public List<GdsPoint> Points { get; set; } = new();
    public bool ExternalData { get; set; }
    public bool TemplateData { get; set; }
    public int PlexNumber { get; set; }

    /// <inheritdoc />
    public GdsBoundingBox GetBoundingBox(GdsStructure.StructureProvider structureProvider)
    {
        var structure = structureProvider(StructureName);
        if (structure == null)
            throw new KeyNotFoundException($"Structure {StructureName} not found");

        var boundingBox = structure.GetBoundingBox(structureProvider);
        if (boundingBox.IsEmpty) return boundingBox;
        var boundingBoxes = Points.Select(p => new GdsBoundingBox(p + boundingBox.Min, p + boundingBox.Max));
        return new GdsBoundingBox(boundingBoxes);
    }
}