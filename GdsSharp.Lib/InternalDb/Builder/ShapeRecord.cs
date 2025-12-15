namespace GdsSharp.Lib.InternalDb;

public readonly record struct ShapeRecord(
    CellId Cell,
    ShapeKind Kind,
    short Layer,
    short DataType,
    long VertexOffset,
    int VertexCount,
    GdsBoundingBox BoundingBox,
    int? Width
);