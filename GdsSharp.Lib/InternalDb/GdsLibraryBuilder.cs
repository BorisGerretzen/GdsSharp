using GdsSharp.Lib.InternalDb.BoundingBox;
using GdsSharp.Lib.InternalDb.Builder;
using GdsSharp.Lib.InternalDb.VertexStore;
using GdsSharp.Lib.Old.NonTerminals.Enum;
using GdsSharp.Lib.Parsing.Models;

namespace GdsSharp.Lib.InternalDb;

public enum ElementKind
{
    Boundary,
    Path,
    SRef,
    ARef,
    Text,
    Node,
    Box
}

public readonly record struct ElementRecord(CellId Cell, GdsElementCommon Common, ElementKind Kind, int Index, GdsBoundingBox? BoundingBox);

public readonly record struct BoundaryPayload(short Layer, short DataType, long VertexOffset, int VertexCount);

public readonly record struct PathPayload(short Layer, short DataType, GdsPathType? PathType, int? Width, long VertexOffset, int VertexCount);

public readonly record struct SRefPayload(CellId Parent, string TargetName, GdsStransInfo? Strans, GdsPoint Origin);

public readonly record struct ARefPayload(CellId Parent, string TargetName, GdsStransInfo? Strans, int Rows, int Columns, GdsPoint RowVector, GdsPoint ColumnVector, GdsPoint Origin);

public readonly record struct TextPayload(short Layer, short TextType, PresentationInfo? Presentation, GdsPathType? PathType, int? Width, GdsStransInfo? Strans, GdsPoint Origin, string Text);

public readonly record struct NodePayload(short Layer, short NodeType, long VertexOffset, int VertexCount);

public readonly record struct BoxPayload(short Layer, short BoxType, long VertexOffset, int VertexCount);

public class GdsLibraryBuilder(IGdsVertexStoreWriter vertexWriter)
{
    private readonly List<GdsStructure> _structures = [];

    private readonly List<ElementRecord> _elements = [];
    private readonly List<BoundaryPayload> _boundaries = [];
    private readonly List<PathPayload> _paths = [];
    private readonly List<SRefPayload> _structureReferences = [];
    private readonly List<ARefPayload> _arrayReferences = [];
    private readonly List<TextPayload> _texts = [];
    private readonly List<NodePayload> _nodes = [];
    private readonly List<BoxPayload> _boxes = [];

    private readonly List<PropertyRecord> _properties = [];

    private GdsLibraryInfo? _info;

    public void SetInfo(GdsLibraryInfo info)
    {
        _info = info;
    }

    public CellId AddStructure(GdsStructureInfo structureInfo)
    {
        var idx = _structures.Count;
        _structures.Add(new GdsStructure(
            Info: structureInfo,
            BoundingBox: null));
        return new CellId(idx);
    }

    public int AddStructureReference(CellId parentId, GdsElementCommon common, string targetName, GdsStransInfo? strans, GdsPoint origin)
    {
        var elementId = _elements.Count;
        var elementRecord = new ElementRecord(parentId, common, ElementKind.SRef, _structureReferences.Count, null);
        _elements.Add(elementRecord);

        var sref = new SRefPayload(parentId, targetName, strans, origin);
        _structureReferences.Add(sref);

        return elementId;
    }

    public int AddArrayReference(CellId parentId, GdsElementCommon common, string targetName, GdsStransInfo? strans, int rows, int columns, GdsPoint rowVector, GdsPoint columnVector,
        GdsPoint origin)
    {
        var elementId = _elements.Count;
        var elementRecord = new ElementRecord(parentId, common, ElementKind.ARef, _arrayReferences.Count, null);
        _elements.Add(elementRecord);

        var aref = new ARefPayload(parentId, targetName, strans, rows, columns, rowVector, columnVector, origin);
        _arrayReferences.Add(aref);

        return elementId;
    }

    public int AddBoundary(CellId cell, GdsElementCommon common, short layer, short dataType, ReadOnlySpan<GdsPoint> points)
    {
        var elementId = _elements.Count;
        var elementRecord = new ElementRecord(cell, common, ElementKind.Boundary, _boundaries.Count, GdsBoundingBox.FromPoints(points));
        _elements.Add(elementRecord);

        var vertexOffset = vertexWriter.Write(points);

        var boundary = new BoundaryPayload(layer, dataType, vertexOffset, points.Length);
        _boundaries.Add(boundary);

        return elementId;
    }

    public int AddBox(CellId cell, GdsElementCommon common, short layer, short dataType, ReadOnlySpan<GdsPoint> points)
    {
        var elementId = _elements.Count;
        var elementRecord = new ElementRecord(cell, common, ElementKind.Box, _boxes.Count, GdsBoundingBox.FromPoints(points));
        _elements.Add(elementRecord);

        var vertexOffset = vertexWriter.Write(points);

        var box = new BoxPayload(layer, dataType, vertexOffset, points.Length);
        _boxes.Add(box);

        return elementId;
    }

    public int AddPath(CellId cell, GdsElementCommon common, short layer, short dataType, ReadOnlySpan<GdsPoint> points, int? width, GdsPathType? pathType)
    {
        var halfWidth = (width ?? 0) / 2;
        var boundingBox = GdsBoundingBox.FromPoints(points);
        boundingBox = new GdsBoundingBox(
            new GdsPoint(boundingBox.Min.X - halfWidth, boundingBox.Min.Y - halfWidth),
            new GdsPoint(boundingBox.Max.X + halfWidth, boundingBox.Max.Y + halfWidth)
        );

        var elementId = _elements.Count;
        var elementRecord = new ElementRecord(cell, common, ElementKind.Path, _paths.Count, boundingBox);
        _elements.Add(elementRecord);

        var vertexOffset = vertexWriter.Write(points);

        var path = new PathPayload(layer, dataType, pathType, width, vertexOffset, points.Length);
        _paths.Add(path);

        return elementId;
    }

    public int AddNode(CellId cell, GdsElementCommon common, short layer, short dataType, ReadOnlySpan<GdsPoint> points)
    {
        var elementId = _elements.Count;
        var elementRecord = new ElementRecord(cell, common, ElementKind.Node, _nodes.Count, GdsBoundingBox.FromPoints(points));
        _elements.Add(elementRecord);

        var vertexOffset = vertexWriter.Write(points);

        var node = new NodePayload(layer, dataType, vertexOffset, points.Length);
        _nodes.Add(node);

        return elementId;
    }

    public int AddText(CellId cell, GdsElementCommon common, short layer, short textType, PresentationInfo? presentation, GdsPathType? pathType, int? width, GdsStransInfo? strans, GdsPoint origin,
        string text)
    {
        var elementId = _elements.Count;
        var elementRecord = new ElementRecord(cell, common, ElementKind.Text, _texts.Count, null);
        _elements.Add(elementRecord);

        var textRecord = new TextPayload(layer, textType, presentation, pathType, width, strans, origin, text);
        _texts.Add(textRecord);

        return elementId;
    }

    public void AddElementProperty(int elementId, short attribute, string value)
    {
        var propertyRecord = new PropertyRecord(
            ElementId: elementId,
            Attribute: attribute,
            Value: value);
        _properties.Add(propertyRecord);
    }

    public GdsLibrary Build()
    {
        if (!_info.HasValue)
            throw new InvalidOperationException("Library info must be set before building the library.");

        BuildInitialStructureBBs();
        ComputeBBs();
        // ApplyReferences();
        return new GdsLibrary(_info.Value, _structures, _elements, _boundaries, _paths, _structureReferences, _arrayReferences, _texts, _nodes, _boxes, _properties);
    }

    // private void ApplyReferences()
    // {
    //     var structureMap = new Dictionary<string, int>(StringComparer.Ordinal);
    //     for (var i = 0; i < _structures.Count; i++)
    //         structureMap[_structures[i].Info.Name] = i;
    //
    //     var numStructures = _structures.Count;
    //
    //     var refsByParent = new Dictionary<int, List<ResolvedRef>>();
    //
    //     foreach (var r in _structureReferences)
    //     {
    //         var parentId = r.Parent.Id;
    //
    //         if (!structureMap.TryGetValue(r.TargetName, out var childId))
    //             throw new InvalidOperationException($"Unknown structure reference target '{r.TargetName}' referenced by cell {parentId}.");
    //
    //         refsByParent[parentId].Add(new ResolvedRef(childId, r.Transform));
    //         parentsByChild[childId].Add(parentId);
    //     }
    //
    // }

    private void ComputeBBs()
    {
        var numStructures = _structures.Count;
        var structureMap = new Dictionary<string, int>(StringComparer.Ordinal);
        for (var i = 0; i < _structures.Count; i++)
            structureMap[_structures[i].Info.Name] = i;
        
        var structureReferencesByParent = new List<(CellId Child, GdsStransInfo? Strans, GdsPoint Origin)>[numStructures];
        var arrayReferencesByParent = new List<(GdsPoint RowVector, GdsPoint ColVector, GdsPoint Origin)>[numStructures];
        for (var i = 0; i < numStructures; i++)
        {
            structureReferencesByParent[i] = [];
            arrayReferencesByParent[i] = [];
        }
        
        var children = new List<CellId>[numStructures];
        for (var i = 0; i < numStructures; i++) children[i] = [];

        var indegree = new int[numStructures];

        // Build dependency graph
        foreach (var r in _structureReferences)
        {
            if (!structureMap.TryGetValue(r.TargetName, out var childId)) throw new InvalidOperationException($"Unknown structure reference target '{r.TargetName}'.");
            var parentId = r.Parent.Id;
            structureReferencesByParent[parentId].Add((new CellId(childId), r.Strans, r.Origin));
            children[parentId].Add(new CellId(childId));
            indegree[parentId]++;
        }

        foreach (var r in _arrayReferences)
        {
            if (!structureMap.TryGetValue(r.TargetName, out var childId)) throw new InvalidOperationException($"Unknown array reference target '{r.TargetName}'.");
            var parentId = r.Parent.Id;
            arrayReferencesByParent[parentId].Add((r.RowVector, r.ColumnVector, r.Origin));
            children[parentId].Add(new CellId(childId));
            indegree[parentId]++;
        }
        
        // Topological sort zero indegree first, build bounding boxes leaves up
        var queue = new Queue<int>();
        for (var i = 0; i < numStructures; i++)
        {
            if (indegree[i] == 0)
                queue.Enqueue(i);
        }
        
        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            var currentBox = _structures[current].BoundingBox;

            // Process structure references
            foreach (var (childId, strans, origin) in structureReferencesByParent[current])
            {
                var childBox = _structures[childId.Id].BoundingBox;
                if (childBox is { IsEmpty: false })
                {
                    var transformedBox = childBox.Value.TransformBoundingBox(strans ?? GdsStransInfo.Default, origin);
                    currentBox = currentBox?.Union(transformedBox) ?? transformedBox;
                }
            }

            // Process array references, rowvec and colvec are pre-transformed
            foreach (var (rowVector, colVector, origin) in arrayReferencesByParent[current])
            {
                var childBox = GdsBoundingBox.FromPoints([origin, colVector, rowVector]);
                currentBox = currentBox?.Union(childBox) ?? childBox;
            }

            _structures[current] = _structures[current] with { BoundingBox = currentBox };

            // Decrease indegree of children and enqueue if zero
            foreach (var child in children[current])
            {
                indegree[child.Id]--;
                if (indegree[child.Id] == 0)
                    queue.Enqueue(child.Id);
            }
        }
    }
    
    private void BuildInitialStructureBBs()
    {
        for (var i = 0; i < _structures.Count; i++)
        {
            var structure = _structures[i];
            var boundingBox = GdsBoundingBox.Empty;

            foreach (var element in _elements.Where(s => s.Cell.Id == i))
            {
                if (!element.BoundingBox.HasValue || element.BoundingBox.Value.IsEmpty) continue;
                boundingBox = boundingBox.Union(element.BoundingBox.Value);
            }

            _structures[i] = structure with { BoundingBox = boundingBox.IsEmpty ? null : boundingBox };
        }
    }

}