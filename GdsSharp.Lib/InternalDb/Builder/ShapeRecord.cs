using GdsSharp.Lib.InternalDb.BoundingBox;
using GdsSharp.Lib.Obsolete.NonTerminals.Enum;

namespace GdsSharp.Lib.InternalDb.Builder;

public readonly record struct ShapeRecord(
    CellId Cell,
    ShapeId Shape,
    ShapeKind Kind,
    short Layer,
    short DataType,
    long VertexOffset,
    int VertexCount,
    GdsBoundingBox BoundingBox,
    int? Width,
    GdsPathType? PathType
);