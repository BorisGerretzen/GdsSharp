using GdsSharp.Lib.InternalDb.BoundingBox;
using GdsSharp.Lib.InternalDb.Builder;
using GdsSharp.Lib.InternalDb.VertexStore;
using GdsSharp.Lib.Old.NonTerminals.Enum;
using GdsSharp.Lib.Parsing.Models;

namespace GdsSharp.Lib.InternalDb;

public class GdsLibraryBuilder(IGdsVertexStoreWriter vertexWriter)
{
    private readonly List<GdsStructure> _structures = [];
    
    private readonly List<ShapeRecord> _shapeRecords = [];
    private readonly List<GdsStructureReference> _structureReferences = [];
    private readonly List<GdsArrayReference> _arrayReferences = [];

    private readonly List<PropertyRecord> _properties = [];
    private readonly List<TextRecord> _textRecords = [];
    
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

    public StructureReferenceId AddStructureReference(CellId parentId, string targetName, GdsTransform transform)
    {
        var idx = _structureReferences.Count;
        _structureReferences.Add(new GdsStructureReference(parentId, targetName, transform));
        return new StructureReferenceId(idx);
    }

    public ArrayReferenceId AddArrayReference(CellId structureId, string targetName, GdsTransform transform, int rows, int columns, GdsPoint rowVector, GdsPoint columnVector)
    {
        var idx = _arrayReferences.Count;
        _arrayReferences.Add(new GdsArrayReference(structureId, targetName, transform, rows, columns, rowVector, columnVector));
        return new ArrayReferenceId(idx);
    }

    public ShapeId AddBoundary(CellId cell, short layer, short dataType, ReadOnlySpan<GdsPoint> points)
    {
        var id = new ShapeId(_shapeRecords.Count);
        var vertexOffset = vertexWriter.Write(points);
        var shapeRecord = new ShapeRecord(
            Cell: cell,
            Shape: id,
            Kind: ShapeKind.Boundary,
            Layer: layer,
            DataType: dataType,
            VertexOffset: vertexOffset,
            VertexCount: points.Length,
            BoundingBox: GdsBoundingBox.FromPoints(points),
            Width: null,
            PathType: null
        );
        _shapeRecords.Add(shapeRecord);
        return id;
    }

    public ShapeId AddBox(CellId cell, short layer, short dataType, ReadOnlySpan<GdsPoint> points)
    {
        return AddBoundary(cell, layer, dataType, points);
    }

    public ShapeId AddPath(CellId cell, short layer, short dataType, ReadOnlySpan<GdsPoint> points, int? width, GdsPathType? pathType)
    {
        var id = new ShapeId(_shapeRecords.Count);
        var vertexOffset = vertexWriter.Write(points);
        
        var halfWidth = (width ?? 0) / 2;
        var boundingBox = GdsBoundingBox.FromPoints(points);
        boundingBox = new GdsBoundingBox(
            new GdsPoint(boundingBox.Min.X - halfWidth, boundingBox.Min.Y - halfWidth),
            new GdsPoint(boundingBox.Max.X + halfWidth, boundingBox.Max.Y + halfWidth)
        );

        var shapeRecord = new ShapeRecord(
            Cell: cell,
            Shape: id,
            Kind: ShapeKind.Path,
            Layer: layer,
            DataType: dataType,
            VertexOffset: vertexOffset,
            VertexCount: points.Length,
            BoundingBox: boundingBox,
            Width: width,
            PathType: pathType
        );
        _shapeRecords.Add(shapeRecord);
        return id;
    }

    public ShapeId AddNode(CellId cell, short layer, short dataType, ReadOnlySpan<GdsPoint> points)
    {
        var id = new ShapeId(_shapeRecords.Count);
        var vertexOffset = vertexWriter.Write(points);
        var shapeRecord = new ShapeRecord(
            Cell: cell,
            Shape: id,
            Kind: ShapeKind.Node,
            Layer: layer,
            DataType: dataType,
            VertexOffset: vertexOffset,
            VertexCount: points.Length,
            BoundingBox: GdsBoundingBox.FromPoints(points),
            Width: null,
            PathType: null
        );
        _shapeRecords.Add(shapeRecord);
        return id;
    }
    
    public ShapeId AddText(CellId cell, short layer, short textType, string text, PresentationInfo? presentation, GdsPathType? pathType, int? width, GdsTransform transform)
    {
        var id = new ShapeId(_shapeRecords.Count);
        var shapeRecord = new ShapeRecord(
            Cell: cell,
            Shape: id,
            Kind: ShapeKind.Text,
            Layer: layer,
            DataType: textType,
            VertexOffset: -1,
            VertexCount: 0,
            BoundingBox: new GdsBoundingBox(transform.Origin, transform.Origin),
            Width: width,
            PathType: null
        );
        _shapeRecords.Add(shapeRecord);

        var textRecord = new TextRecord(id, text, presentation, pathType, transform.Strans, transform.Origin);
        _textRecords.Add(textRecord);
        
        return id;
    }
    
    public void AddElementProperty(PropertyRecord propertyRecord)
    {
        _properties.Add(propertyRecord);
    }
    
    public GdsLibrary Build()
    {
        if (!_info.HasValue)
            throw new InvalidOperationException("Library info must be set before building the library.");
        
        BuildInitialStructureBBs();
        ApplyReferences();
        return new GdsLibrary(_info.Value, _structures, _shapeRecords, _structureReferences, _arrayReferences, _properties, _textRecords);
    }

    private void ApplyReferences()
    {
        var structureMap = new Dictionary<string, int>(StringComparer.Ordinal);
        for (var i = 0; i < _structures.Count; i++)
            structureMap[_structures[i].Info.Name] = i;

        var numStructures = _structures.Count;

        var refsByParent = new List<ResolvedRef>[numStructures];
        var parentsByChild = new List<int>[numStructures];

        for (var i = 0; i < numStructures; i++)
        {
            refsByParent[i] = [];
            parentsByChild[i] = [];
        }

        foreach (var r in _structureReferences)
        {
            if (!structureMap.TryGetValue(r.TargetName, out var childId))
                throw new InvalidOperationException($"Unknown structure reference target '{r.TargetName}' referenced by cell {r.Parent.Id}.");

            var parentId = r.Parent.Id;

            refsByParent[parentId].Add(new ResolvedRef(childId, r.Transform));
            parentsByChild[childId].Add(parentId);
        }

        ComputeStructureBoundingBoxes(refsByParent, parentsByChild);
    }
    
    private void ComputeStructureBoundingBoxes(
        List<ResolvedRef>[] refsByParent,
        List<int>[] parentsByChild)
    {
        var numStructures = _structures.Count;

        var depsRemaining = new int[numStructures];
        for (var i = 0; i < numStructures; i++)
            depsRemaining[i] = refsByParent[i].Count;

        var finalBox = new GdsBoundingBox?[numStructures];

        var q = new Queue<int>();
        for (var i = 0; i < numStructures; i++)
        {
            if (depsRemaining[i] == 0) 
                q.Enqueue(i);
        }

        var computed = 0;

        while (q.TryDequeue(out var id))
        {
            var bbox = _structures[id].BoundingBox;

            foreach (var rr in refsByParent[id])
            {
                var childBox = finalBox[rr.TargetId]
                               ?? throw new InvalidOperationException("Dependency order broken: child bbox missing.");
                bbox = rr.Transform.TransformBoundingBox(childBox);
            }

            // Store final
            finalBox[id] = bbox;
            _structures[id] = _structures[id] with { BoundingBox = bbox };

            computed++;

            // Unblock parents that depended on this
            foreach (var parent in parentsByChild[id])
            {
                if (--depsRemaining[parent] == 0)
                    q.Enqueue(parent);
            }
        }

        if (computed != numStructures)
            throw new InvalidOperationException("Structure reference cycle detected (recursive references). Bounding boxes cannot be resolved with a DAG evaluation.");
    }

    private void BuildInitialStructureBBs()
    {
        for(var i = 0; i < _structures.Count; i++)
        {
            var structure = _structures[i];
            var boundingBox = GdsBoundingBox.Empty;

            foreach (var shape in _shapeRecords.Where(s => s.Cell.Id == i))
            {
                boundingBox = boundingBox.Union(shape.BoundingBox);
            }
            
            _structures[i] = structure with { BoundingBox = boundingBox.IsEmpty ? null : boundingBox };
        }
    }

    private readonly record struct ResolvedRef(int TargetId, GdsTransform Transform);
}