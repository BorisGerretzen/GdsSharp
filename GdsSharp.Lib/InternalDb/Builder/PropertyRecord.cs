namespace GdsSharp.Lib.InternalDb;

public readonly record struct PropertyRecord(
    short Attribute,
    string Value,
    ShapeId? ShapeId,
    StructureReferenceId? StructureReferenceId,
    ArrayReferenceId? ArrayReferenceId
)
{
    public static PropertyRecord ForShape(short attribute, string value, ShapeId shapeId) => new(attribute, value, shapeId, null, null);
    public static PropertyRecord ForStructureReference(short attribute, string value, StructureReferenceId structureReferenceId) => new(attribute, value, null, structureReferenceId, null);
    public static PropertyRecord ForArrayReference(short attribute, string value, ArrayReferenceId arrayReferenceId) => new(attribute, value, null, null, arrayReferenceId);
}