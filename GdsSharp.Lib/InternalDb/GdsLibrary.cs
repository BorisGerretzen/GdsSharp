using GdsSharp.Lib.InternalDb.Builder;
using GdsSharp.Lib.Reading.Models;

namespace GdsSharp.Lib.InternalDb;

public class GdsLibrary(
    GdsLibraryInfo info,
    GdsStructure[] structures,
    ElementRecord[] elements,
    BoundaryPayload[] boundaries,
    PathPayload[] paths,
    SRefPayload[] structureReferences,
    ARefPayload[] arrayReferences,
    TextPayload[] texts,
    NodePayload[] nodes,
    BoxPayload[] boxes,
    PropertyRecord[] properties
)
{
    public GdsLibraryInfo Info { get; } = info;

    public GdsStructure[] Structures => structures;
    public ElementRecord[] Elements => elements;
    public BoundaryPayload[] Boundaries => boundaries;
    public PathPayload[] Paths => paths;
    public SRefPayload[] StructureReferences => structureReferences;
    public ARefPayload[] ArrayReferences => arrayReferences;
    public TextPayload[] Texts => texts;
    public NodePayload[] Nodes => nodes;
    public BoxPayload[] Boxes => boxes;
    public PropertyRecord[] Properties => properties;

    internal Lazy<Dictionary<int, PropertyRecord[]>> PropertiesByElementIndex => new(() =>
    {
        return Properties
            .GroupBy(p => p.ElementId)
            .ToDictionary(g => g.Key, g => g.ToArray());
    });
}