using GdsSharp.Lib.NonTerminals.Abstractions;

namespace GdsSharp.Lib.NonTerminals.Elements;

public class GdsStructureReferenceElement : IGdsElement
{
    private const float Deg2Rad = MathF.PI / 180;
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

        IEnumerable<GdsBoundingBox> boundingBoxes;
        if (Transformation.Angle != 0)
        {
            var (sin, cos) = MathF.SinCos((float)Transformation.Angle * GdsExtensions.Deg2Rad);
            boundingBoxes = Points.Select(p => new GdsBoundingBox(new[]
            {
                p + boundingBox.Min.Rotate(sin, cos),
                p + boundingBox.Max.Rotate(sin, cos)
            }));
        }
        else
        {
            boundingBoxes = Points.Select(p => new GdsBoundingBox(
                p + boundingBox.Min,
                p + boundingBox.Max)
            );
        }

        return new GdsBoundingBox(boundingBoxes);
    }
}