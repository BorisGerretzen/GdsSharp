using GdsSharp.Lib.InternalDb.Builder;
using GdsSharp.Lib.Parsing.Models;

namespace GdsSharp.Lib.InternalDb;

public class GdsLibrary(
    GdsLibraryInfo info,
    List<GdsStructure> structures,
    List<ElementRecord> elements,
    List<BoundaryPayload> boundaries,
    List<PathPayload> paths,
    List<SRefPayload> structureReferences,
    List<ARefPayload> arrayReferences,
    List<TextPayload> texts,
    List<NodePayload> nodes,
    List<BoxPayload> boxes,
    List<PropertyRecord> properties
)
{
    public GdsLibraryInfo Info { get; } = info;

    public IReadOnlyList<GdsStructure> Structures => structures;
    public IReadOnlyList<ElementRecord> Elements => elements;
    public IReadOnlyList<BoundaryPayload> Boundaries => boundaries;
    public IReadOnlyList<PathPayload> Paths => paths;
    public IReadOnlyList<SRefPayload> StructureReferences => structureReferences;
    public IReadOnlyList<ARefPayload> ArrayReferences => arrayReferences;
    public IReadOnlyList<TextPayload> Texts => texts;
    public IReadOnlyList<NodePayload> Nodes => nodes;
    public IReadOnlyList<BoxPayload> Boxes => boxes;
    public IReadOnlyList<PropertyRecord> Properties => properties;
}