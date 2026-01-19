using System.Runtime.InteropServices;
using GdsSharp.Lib.Library.BoundingBox;
using GdsSharp.Lib.Library.Builder;
using GdsSharp.Lib.Library.Builder.Payload;
using GdsSharp.Lib.Library.VertexStore;
using GdsSharp.Lib.Reading;
using GdsSharp.Lib.Reading.Enum;
using GdsSharp.Lib.Reading.Models;

namespace GdsSharp.Lib.Library;

public class GdsLibraryBuilder(IGdsVertexStore vertexStore, bool buildBoundingBoxes = false)
{
    private readonly List<ARefPayload> _arrayReferences = [];
    private readonly List<BoundaryPayload> _boundaries = [];
    private readonly List<BoxPayload> _boxes = [];

    private readonly List<ElementRecord> _elements = [];
    private readonly List<NodePayload> _nodes = [];
    private readonly List<PathPayload> _paths = [];

    private readonly List<PropertyRecord> _properties = [];
    private readonly List<SRefPayload> _structureReferences = [];
    private readonly List<MutGdsStructure> _structures = [];
    private readonly List<TextPayload> _texts = [];
    private CellId? _currentStructureId;

    public GdsLibraryInfo? Info { get; set; }

    public CellId AddStructure(GdsStructureInfo structureInfo)
    {
        var idx = _structures.Count;
        _currentStructureId = new CellId(idx);
        _structures.Add(new MutGdsStructure(
            structureInfo,
            _elements.Count,
            0,
            null));
        return _currentStructureId.Value;
    }

    public int AddStructureReference(GdsElementCommon? common, string targetName, GdsStransInfo? strans, GdsPoint origin)
    {
        if (!_currentStructureId.HasValue)
            throw new InvalidOperationException("No current structure. Call AddStructure before adding elements.");

        var elementId = _elements.Count;
        var elementRecord = new ElementRecord(common, GdsElementKind.StructureReference, _structureReferences.Count, null, -1);
        _elements.Add(elementRecord);

        var sref = new SRefPayload(_currentStructureId.Value, targetName, strans, origin);
        _structureReferences.Add(sref);
        IncrementStructureElementCount();

        return elementId;
    }

    public int AddArrayReference(GdsElementCommon? common, string targetName, GdsStransInfo? strans, int rows, int columns, GdsPoint rowVector, GdsPoint columnVector, GdsPoint origin)
    {
        if (!_currentStructureId.HasValue)
            throw new InvalidOperationException("No current structure. Call AddStructure before adding elements.");

        var elementId = _elements.Count;
        var elementRecord = new ElementRecord(common, GdsElementKind.ArrayReference, _arrayReferences.Count, null, -1);
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
        var elementRecord = new ElementRecord(common, GdsElementKind.Boundary, _boundaries.Count, GdsBoundingBox.FromPoints(points), layer);
        _elements.Add(elementRecord);

        var vertexOffset = vertexStore.Write(points);

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

        if (points.Length != GdsGlobals.BoxPointCount) throw new InvalidOperationException("BOX must have exactly 5 points.");
        if (points[0] != points[4]) throw new InvalidOperationException("The first and last points of a BOX must be the same.");

        var elementId = _elements.Count;
        var elementRecord = new ElementRecord(common, GdsElementKind.Box, _boxes.Count, GdsBoundingBox.FromPoints(points), layer);
        _elements.Add(elementRecord);

        var vertexOffset = vertexStore.Write(points);

        var box = new BoxPayload(layer, dataType, vertexOffset);
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
        var elementRecord = new ElementRecord(common, GdsElementKind.Path, _paths.Count, boundingBox, layer);
        _elements.Add(elementRecord);

        var vertexOffset = vertexStore.Write(points);

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
        var elementRecord = new ElementRecord(common, GdsElementKind.Node, _nodes.Count, GdsBoundingBox.FromPoints(points), layer);
        _elements.Add(elementRecord);

        var vertexOffset = vertexStore.Write(points);

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
        var elementRecord = new ElementRecord(common, GdsElementKind.Text, _texts.Count, null, layer);
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
            elementId,
            attribute,
            value);
        _properties.Add(propertyRecord);
    }

    public GdsLibrary Build()
    {
        if (!Info.HasValue)
            throw new InvalidOperationException("Library info must be set before building the library.");

        if (buildBoundingBoxes) ComputeReferenceBoundingBoxes();

        var immutableStructures = new GdsStructure[_structures.Count];
        for (var i = 0; i < _structures.Count; i++) immutableStructures[i] = MutGdsStructure.ToGdsStructure(_structures[i]);

        return new GdsLibrary(vertexStore, Info.Value, immutableStructures, _elements.ToArray(), _boundaries.ToArray(), _paths.ToArray(), _structureReferences.ToArray(),
            _arrayReferences.ToArray(), _texts.ToArray(), _nodes.ToArray(), _boxes.ToArray(), _properties.ToArray());
    }

    private void IncrementStructureElementCount()
    {
        ref var structure = ref CollectionsMarshal.AsSpan(_structures)[_currentStructureId!.Value.Id];
        structure.ElementCount++;
    }

    private void UpdateStructureBoundingBox(CellId cellId, GdsBoundingBox? boundingBox)
    {
        if (!boundingBox.HasValue || !buildBoundingBoxes) return;

        ref var structure = ref CollectionsMarshal.AsSpan(_structures)[cellId.Id];
        structure.BoundingBox = structure.BoundingBox?.Union(boundingBox.Value) ?? boundingBox;
    }

    private void ComputeReferenceBoundingBoxes()
    {
        var numStructures = _structures.Count;
        var structuresSpan = CollectionsMarshal.AsSpan(_structures);
        var structureMap = new Dictionary<string, int>(StringComparer.Ordinal);
        for (var i = 0; i < structuresSpan.Length; i++)
            structureMap[structuresSpan[i].Info.Name] = i;

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
            if (numDeps[i] == 0)
                queue.Enqueue(i);

        var numProcessed = 0;
        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            var currentBox = structuresSpan[current].BoundingBox;
            ref var currentStructure = ref structuresSpan[current];

            // Process structure references
            if (structureReferencesByParent[current] is { } srefs)
                foreach (var (childId, strans, origin) in srefs)
                {
                    var childBox = structuresSpan[childId.Id].BoundingBox;
                    if (childBox is { IsEmpty: false })
                    {
                        var transformedBox = childBox.Value.TransformBoundingBox(strans ?? GdsStransInfo.Default, origin);
                        currentBox = currentBox?.Union(transformedBox) ?? transformedBox;
                    }
                }

            // Process array references, rowvec and colvec are pre-transformed
            if (arrayReferencesByParent[current] is { } arefs)
                foreach (var (rowVector, colVector, origin) in arefs)
                {
                    var p3 = origin + rowVector + colVector;
                    var childBox = GdsBoundingBox.FromPoints([origin, colVector, rowVector, p3]);
                    currentBox = currentBox?.Union(childBox) ?? childBox;
                }

            currentStructure.BoundingBox = currentBox;

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

    private struct MutGdsStructure(GdsStructureInfo info, int elementStartIndex, int elementCount, GdsBoundingBox? boundingBox)
    {
        public readonly GdsStructureInfo Info = info;
        public readonly int ElementStartIndex = elementStartIndex;
        public int ElementCount = elementCount;
        public GdsBoundingBox? BoundingBox = boundingBox;

        public static GdsStructure ToGdsStructure(MutGdsStructure s)
        {
            return new GdsStructure(
                s.Info,
                s.ElementStartIndex,
                s.ElementCount,
                s.BoundingBox);
        }
    }
}