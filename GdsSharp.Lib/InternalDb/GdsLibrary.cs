using GdsSharp.Lib.InternalDb.Builder;
using GdsSharp.Lib.Parsing.Models;

namespace GdsSharp.Lib.InternalDb;

public class GdsLibrary(
    GdsLibraryInfo info,
    List<GdsStructure> structures,
    List<ShapeRecord> shapeRecords,
    List<GdsStructureReference> structureReferences,
    List<GdsArrayReference> arrayReferences,
    List<PropertyRecord> properties,
    List<TextRecord> textRecords)
{
    public GdsLibraryInfo Info { get; } = info;

    public IReadOnlyList<GdsStructure> Structures => structures;
    public IReadOnlyList<ShapeRecord> ShapeRecords => shapeRecords;
    public IReadOnlyList<GdsStructureReference> StructureReferences => structureReferences;
    public IReadOnlyList<GdsArrayReference> ArrayReferences => arrayReferences;
    public IReadOnlyList<PropertyRecord> Properties => properties;
    public IReadOnlyList<TextRecord> TextRecords => textRecords;
}