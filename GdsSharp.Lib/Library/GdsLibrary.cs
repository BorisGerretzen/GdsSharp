using GdsSharp.Lib.Library.Builder;
using GdsSharp.Lib.Library.Builder.Payload;
using GdsSharp.Lib.Library.VertexStore;
using GdsSharp.Lib.Reading;
using GdsSharp.Lib.Reading.Consumer;
using GdsSharp.Lib.Reading.Models;
using GdsSharp.Lib.Reading.TokenStream;

namespace GdsSharp.Lib.Library;

public class GdsLibrary
{
    private readonly IGdsVertexStore _vertexStore; 
    private readonly GdsStructure[] _structures;
    private readonly ElementRecord[] _elements;
    private readonly BoundaryPayload[] _boundaries;
    private readonly PathPayload[] _paths;
    private readonly SRefPayload[] _structureReferences;
    private readonly ARefPayload[] _arrayReferences;
    private readonly TextPayload[] _texts;
    private readonly NodePayload[] _nodes;
    private readonly BoxPayload[] _boxes;
    private readonly PropertyRecord[] _properties;

    internal GdsLibrary(
        IGdsVertexStore vertexStore,
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
        PropertyRecord[] properties)
    {
        _vertexStore = vertexStore;
        _structures = structures;
        _elements = elements;
        _boundaries = boundaries;
        _paths = paths;
        _structureReferences = structureReferences;
        _arrayReferences = arrayReferences;
        _texts = texts;
        _nodes = nodes;
        _boxes = boxes;
        _properties = properties;
        Info = info;
    }

    public GdsLibraryInfo Info { get; }

    public GdsStructure[] Structures => _structures;
    public ElementRecord[] Elements => _elements;
    public BoundaryPayload[] Boundaries => _boundaries;
    public PathPayload[] Paths => _paths;
    public SRefPayload[] StructureReferences => _structureReferences;
    public ARefPayload[] ArrayReferences => _arrayReferences;
    public TextPayload[] Texts => _texts;
    public NodePayload[] Nodes => _nodes;
    public BoxPayload[] Boxes => _boxes;
    public PropertyRecord[] Properties => _properties;

    internal Lazy<Dictionary<int, PropertyRecord[]>> PropertiesByElementIndex => new(() =>
    {
        return Properties
            .GroupBy(p => p.ElementId)
            .ToDictionary(g => g.Key, g => g.ToArray());
    });

    public static GdsLibrary FromFile(string path)
    {
        using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.SequentialScan); 
        return FromStream(fs);
    }

    public static GdsLibrary FromStream(Stream stream)
    {
        var vertexStore = new ChunkedVertexStore();
        var consumer = new GdsLibraryBuilderConsumer(vertexStore);
        var tokenStream = new GdsTokenStream(stream);
        var parser = new GdsParser(tokenStream);
        parser.Parse(consumer);
        return consumer.Library;
    }
}