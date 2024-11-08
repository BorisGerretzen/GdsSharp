using GdsSharp.Lib.NonTerminals.Abstractions;
using GdsSharp.Lib.NonTerminals.Enum;

namespace GdsSharp.Lib.NonTerminals.Elements;

public class GdsPathElement : IGdsLayeredElement
{
    public short DataType { get; set; }
    public GdsPathType PathType { get; set; } = GdsPathType.Square;
    public int Width { get; set; }
    public bool IsAbsoluteWidth { get; set; }
    public List<GdsPoint> Points { get; set; } = new();
    public bool ExternalData { get; set; }
    public bool TemplateData { get; set; }
    public int PlexNumber { get; set; }
    public short Layer { get; set; }

    /// <inheritdoc />
    public GdsBoundingBox GetBoundingBox(GdsStructure.StructureProvider structureProvider)
    {
        var segments = Points.Zip(Points.Skip(1));
        var coordinates = segments
            .Select(segment =>
            {
                var (start, end) = segment;
                var dx = end.X - start.X;
                var dy = end.Y - start.Y;
                return (start, end, dy, dy: -dx);
            })
            .SelectMany(row =>
            {
                return new[]
                {
                    new GdsPoint(row.start.X + row.dy * Width / 2, row.start.Y + row.dy * Width / 2),
                    new GdsPoint(row.start.X - row.dy * Width / 2, row.start.Y - row.dy * Width / 2),
                    new GdsPoint(row.end.X - row.dy * Width / 2, row.end.Y - row.dy * Width / 2),
                    new GdsPoint(row.end.X + row.dy * Width / 2, row.end.Y + row.dy * Width / 2)
                };
            });

        return new GdsBoundingBox(coordinates);
    }
}