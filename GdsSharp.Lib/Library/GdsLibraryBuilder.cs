using GdsSharp.Lib.Library.BoundingBox;
using GdsSharp.Lib.Library.Builder;
using GdsSharp.Lib.Library.VertexStore;
using GdsSharp.Lib.Obsolete.NonTerminals.Enum;
using GdsSharp.Lib.Reading.Models;

namespace GdsSharp.Lib.Library;

public class GdsLibraryBuilder(IGdsVertexStore vertexWriter)
{
    private readonly List<ARefPayload> _arrayReferences = [];
    private readonly List<BoundaryPayload> _boundaries = [];
    private readonly List<BoxPayload> _boxes = [];

    private readonly List<ElementRecord> _elements = [];
    private readonly List<NodePayload> _nodes = [];
    private readonly List<PathPayload> _paths = [];

    private readonly List<PropertyRecord> _properties = [];
    private readonly List<SRefPayload> _structureReferences = [];
    private readonly List<GdsStructure> _structures = [];
    private readonly List<TextPayload> _texts = [];
    private CellId? _currentStructureId;

    public GdsLibraryInfo? Info { get; set; }
    
    public CellId AddStructure(GdsStructureInfo structureInfo)
    {
        var idx = _structures.Count;
        _currentStructureId = new CellId(idx);
        _structures.Add(new GdsStructure(
            Info: structureInfo,
            _elements.Count,
            0,
            BoundingBox: null));
        return _currentStructureId.Value;
    }

    public int AddStructureReference(GdsElementCommon? common, string targetName, GdsStransInfo? strans, GdsPoint origin)
    {
        if (!_currentStructureId.HasValue)
            throw new InvalidOperationException("No current structure. Call AddStructure before adding elements.");

        var elementId = _elements.Count;
        var elementRecord = new ElementRecord(common, ElementKind.SRef, _structureReferences.Count, null);
        _elements.Add(elementRecord);

        var sref = new SRefPayload(_currentStructureId.Value, targetName, strans, origin);
        _structureReferences.Add(sref);
        IncrementStructureElementCount();

        return elementId;
    }

    public int AddArrayReference(GdsElementCommon? common, string targetName, GdsStransInfo? strans, int rows, int columns, GdsPoint rowVector,
        GdsPoint columnVector,
        GdsPoint origin)
    {
        if (!_currentStructureId.HasValue)
            throw new InvalidOperationException("No current structure. Call AddStructure before adding elements.");

        var elementId = _elements.Count;
        var elementRecord = new ElementRecord(common, ElementKind.ARef, _arrayReferences.Count, null);
        _elements.Add(elementRecord);

        var aref = new ARefPayload(_currentStructureId.Value, targetName, strans, rows, columns, rowVector, columnVector, origin);
        _arrayReferences.Add(aref);
        IncrementStructureElementCount();

        return elementId;
    }

    public int AddBoundary(GdsElementCommon? common, short layer, short dataType, ReadOnlySpan<GdsPoint> points)
    {
        if (!_currentStructureId.HasValue)
            throw new InvalidOperationException("No current structure. Call AddStructure before adding elements.");

        var elementId = _elements.Count;
        var elementRecord = new ElementRecord(common, ElementKind.Boundary, _boundaries.Count, GdsBoundingBox.FromPoints(points));
        _elements.Add(elementRecord);

        var vertexOffset = vertexWriter.Write(points);

        var boundary = new BoundaryPayload(layer, dataType, vertexOffset, points.Length);
        _boundaries.Add(boundary);

        UpdateStructureBoundingBox(_currentStructureId.Value, elementRecord.BoundingBox);
        IncrementStructureElementCount();

        return elementId;
    }

    public int AddBox(GdsElementCommon? common, short layer, short dataType, ReadOnlySpan<GdsPoint> points)
    {
        if (!_currentStructureId.HasValue)
            throw new InvalidOperationException("No current structure. Call AddStructure before adding elements.");

        var elementId = _elements.Count;
        var elementRecord = new ElementRecord(common, ElementKind.Box, _boxes.Count, GdsBoundingBox.FromPoints(points));
        _elements.Add(elementRecord);

        var vertexOffset = vertexWriter.Write(points);

        var box = new BoxPayload(layer, dataType, vertexOffset, points.Length);
        _boxes.Add(box);

        UpdateStructureBoundingBox(_currentStructureId.Value, elementRecord.BoundingBox);
        IncrementStructureElementCount();

        return elementId;
    }

    public int AddPath(GdsElementCommon? common, short layer, short dataType, ReadOnlySpan<GdsPoint> points, int? width, GdsPathType? pathType)
    {
        if (!_currentStructureId.HasValue)
            throw new InvalidOperationException("No current structure. Call AddStructure before adding elements.");

        var halfWidth = (width ?? 0) / 2;
        var boundingBox = GdsBoundingBox.FromPoints(points);
        boundingBox = new GdsBoundingBox(
            new GdsPoint(boundingBox.Min.X - halfWidth, boundingBox.Min.Y - halfWidth),
            new GdsPoint(boundingBox.Max.X + halfWidth, boundingBox.Max.Y + halfWidth)
        );

        var elementId = _elements.Count;
        var elementRecord = new ElementRecord(common, ElementKind.Path, _paths.Count, boundingBox);
        _elements.Add(elementRecord);

        var vertexOffset = vertexWriter.Write(points);

        var path = new PathPayload(layer, dataType, pathType, width, vertexOffset, points.Length);
        _paths.Add(path);

        UpdateStructureBoundingBox(_currentStructureId.Value, elementRecord.BoundingBox);
        IncrementStructureElementCount();

        return elementId;
    }

    public int AddNode(GdsElementCommon? common, short layer, short dataType, ReadOnlySpan<GdsPoint> points)
    {
        if (!_currentStructureId.HasValue)
            throw new InvalidOperationException("No current structure. Call AddStructure before adding elements.");

        var elementId = _elements.Count;
        var elementRecord = new ElementRecord(common, ElementKind.Node, _nodes.Count, GdsBoundingBox.FromPoints(points));
        _elements.Add(elementRecord);

        var vertexOffset = vertexWriter.Write(points);

        var node = new NodePayload(layer, dataType, vertexOffset, points.Length);
        _nodes.Add(node);

        UpdateStructureBoundingBox(_currentStructureId.Value, elementRecord.BoundingBox);
        IncrementStructureElementCount();

        return elementId;
    }

    public int AddText(GdsElementCommon? common, short layer, short textType, PresentationInfo? presentation, GdsPathType? pathType, int? width, GdsStransInfo? strans,
        GdsPoint origin,
        string text)
    {
        if (!_currentStructureId.HasValue)
            throw new InvalidOperationException("No current structure. Call AddStructure before adding elements.");

        var elementId = _elements.Count;
        var elementRecord = new ElementRecord(common, ElementKind.Text, _texts.Count, null);
        _elements.Add(elementRecord);

        var textRecord = new TextPayload(layer, textType, presentation, pathType, width, strans, origin, text);
        _texts.Add(textRecord);

        UpdateStructureBoundingBox(_currentStructureId.Value, elementRecord.BoundingBox);
        IncrementStructureElementCount();

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
        if (!Info.HasValue)
            throw new InvalidOperationException("Library info must be set before building the library.");

        ComputeReferenceBoundingBoxes();
        return new GdsLibrary(Info.Value, _structures.ToArray(), _elements.ToArray(), _boundaries.ToArray(), _paths.ToArray(), _structureReferences.ToArray(),
            _arrayReferences.ToArray(), _texts.ToArray(), _nodes.ToArray(), _boxes.ToArray(), _properties.ToArray());
    }

    private void IncrementStructureElementCount()
    {
        var structure = _structures[_currentStructureId!.Value.Id];
        structure = structure with { ElementCount = structure.ElementCount + 1 };
        _structures[_currentStructureId!.Value.Id] = structure;
    }

    private void UpdateStructureBoundingBox(CellId cellId, GdsBoundingBox? boundingBox)
    {
        var structure = _structures[cellId.Id];
        if (boundingBox.HasValue)
        {
            structure = structure with
            {
                BoundingBox = structure.BoundingBox?.Union(boundingBox.Value) ?? boundingBox
            };
            _structures[cellId.Id] = structure;
        }
    }

    private void ComputeReferenceBoundingBoxes()
    {
        var numStructures = _structures.Count;
        var structureMap = new Dictionary<string, int>(StringComparer.Ordinal);
        for (var i = 0; i < _structures.Count; i++)
            structureMap[_structures[i].Info.Name] = i;

        var structureReferencesByParent = new List<(CellId Child, GdsStransInfo? Strans, GdsPoint Origin)>?[numStructures];
        var arrayReferencesByParent = new List<(GdsPoint RowVector, GdsPoint ColVector, GdsPoint Origin)>?[numStructures];

        var parents = new List<CellId>[numStructures];
        for (var i = 0; i < numStructures; i++) parents[i] = [];

        var numDeps = new int[numStructures];

        // Build dependency graph
        foreach (var r in _structureReferences)
        {
            if (!structureMap.TryGetValue(r.TargetName, out var childId)) throw new InvalidOperationException($"Unknown structure reference target '{r.TargetName}'.");
            var parentId = r.Parent.Id;
            structureReferencesByParent[parentId] ??= [];
            structureReferencesByParent[parentId]!.Add((new CellId(childId), r.Strans, r.Origin));
            parents[childId].Add(r.Parent);
            numDeps[parentId]++;
        }

        foreach (var r in _arrayReferences)
        {
            if (!structureMap.TryGetValue(r.TargetName, out var childId)) throw new InvalidOperationException($"Unknown array reference target '{r.TargetName}'.");
            var parentId = r.Parent.Id;
            arrayReferencesByParent[parentId] ??= [];
            arrayReferencesByParent[parentId]!.Add((r.RowVector, r.ColumnVector, r.Origin));
            parents[childId].Add(r.Parent);
            numDeps[parentId]++;
        }

        // Topological sort zero indegree first, build bounding boxes leaves up
        var queue = new Queue<int>();
        for (var i = 0; i < numStructures; i++)
        {
            if (numDeps[i] == 0)
                queue.Enqueue(i);
        }

        var numProcessed = 0;
        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            var currentBox = _structures[current].BoundingBox;

            // Process structure references
            if (structureReferencesByParent[current] is { } srefs)
            {
                foreach (var (childId, strans, origin) in srefs)
                {
                    var childBox = _structures[childId.Id].BoundingBox;
                    if (childBox is { IsEmpty: false })
                    {
                        var transformedBox = childBox.Value.TransformBoundingBox(strans ?? GdsStransInfo.Default, origin);
                        currentBox = currentBox?.Union(transformedBox) ?? transformedBox;
                    }
                }
            }

            // Process array references, rowvec and colvec are pre-transformed
            if (arrayReferencesByParent[current] is { } arefs)
            {
                foreach (var (rowVector, colVector, origin) in arefs)
                {
                    var childBox = GdsBoundingBox.FromPoints([origin, colVector, rowVector]);
                    currentBox = currentBox?.Union(childBox) ?? childBox;
                }
            }

            _structures[current] = _structures[current] with { BoundingBox = currentBox };

            // Decrease dependencies of parents
            foreach (var parentId in parents[current])
            {
                numDeps[parentId.Id]--;
                if (numDeps[parentId.Id] == 0)
                    queue.Enqueue(parentId.Id);
            }

            numProcessed++;
        }

        if (numProcessed != numStructures)
            throw new InvalidOperationException("Cyclic structure references detected when computing bounding boxes.");
    }
}