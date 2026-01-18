using GdsSharp.Lib.Library.Builder;
using GdsSharp.Lib.Library.Builder.Payload;
using GdsSharp.Lib.Library.VertexStore;
using GdsSharp.Lib.Library.Views;
using GdsSharp.Lib.Reading;
using GdsSharp.Lib.Reading.Consumer;
using GdsSharp.Lib.Reading.Models;
using GdsSharp.Lib.Reading.TokenStream;
using GdsSharp.Lib.Writing;

namespace GdsSharp.Lib.Library;

public class GdsLibrary
{
    internal readonly IGdsVertexStore VertexStore; 
    internal readonly GdsStructure[] Structures;
    internal readonly ElementRecord[] Elements;
    internal readonly BoundaryPayload[] Boundaries;
    internal readonly PathPayload[] Paths;
    internal readonly SRefPayload[] StructureReferences;
    internal readonly ARefPayload[] ArrayReferences;
    internal readonly TextPayload[] Texts;
    internal readonly NodePayload[] Nodes;
    internal readonly BoxPayload[] Boxes;
    internal readonly PropertyRecord[] Properties;

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
        VertexStore = vertexStore;
        Structures = structures;
        Elements = elements;
        Boundaries = boundaries;
        Paths = paths;
        StructureReferences = structureReferences;
        ArrayReferences = arrayReferences;
        Texts = texts;
        Nodes = nodes;
        Boxes = boxes;
        Properties = properties;
        Info = info;
    }

    public GdsLibraryInfo Info { get; }

    private Dictionary<int, PropertyRecord[]>? _elementPropertiesIndex;
    private Dictionary<string, int>? _structureIndex;
    
    public LibraryView AsView() => new(this);
    
    public void WriteToFile(string path)
    {
        using var fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None, 4096, FileOptions.SequentialScan); 
        WriteToStream(fs);
    }
    
    public void WriteToStream(Stream stream)
    {
        var writer = new GdsWriter(stream);
        writer.Write(this);
    }
    
    public static GdsLibrary FromFile(string path, Action<GdsLibraryBuilderOptions>? configure = null)
    {
        using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.SequentialScan); 
        return FromStream(fs, configure);
    }

    public static GdsLibrary FromStream(Stream stream, Action<GdsLibraryBuilderOptions>? configure = null)
    {
        var options = new GdsLibraryBuilderOptions();
        configure?.Invoke(options);

        IGdsVertexStore store = options.VertexStoreType switch
        {
            VertexStoreType.Memory => new ChunkedVertexStore(),
            VertexStoreType.Disk => new DiskVertexStore(),
            _ => throw new ArgumentOutOfRangeException()
        };
        
        var consumer = new GdsLibraryBuilderConsumer(store, buildBoundingBoxes: options.BuildBoundingBoxes);
        var tokenStream = new GdsTokenStream(stream, bufferSize: options.ReadBufferSize);
        var parser = new GdsParser(tokenStream);
        parser.Parse(consumer);
        return consumer.Library;
    }
    
    internal bool TryGetStructureIndex(string structureName, out int index)
    {
        if(_structureIndex == null)
        {
            BuildStructureIndex();
        }
        
        return _structureIndex!.TryGetValue(structureName, out index);
    }
    
    internal bool TryGetElementProperties(int elementId, out PropertyRecord[] properties)
    {
        if(_elementPropertiesIndex == null)
        {
            BuildElementPropertiesIndex();
        }
        
        return _elementPropertiesIndex!.TryGetValue(elementId, out properties!);
    }
    
    private void BuildElementPropertiesIndex()
    {
        _elementPropertiesIndex = Properties
            .GroupBy(p => p.ElementId)
            .ToDictionary(g => g.Key, g => g.ToArray());
    }
    
    private void BuildStructureIndex()
    {
        _structureIndex = new Dictionary<string, int>();
        for (var i = 0; i < Structures.Length; i++)
        {
            var structure = Structures[i];
            _structureIndex[structure.Info.Name] = i;
        }
    }
}