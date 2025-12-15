using GdsSharp.Lib.InternalDb;
using GdsSharp.Lib.InternalDb.VertexStore;
using GdsSharp.Lib.Old.NonTerminals.Enum;
using GdsSharp.Lib.Parsing.Models;

namespace GdsSharp.Lib.Parsing.Consumer;

public class InternalDbConsumer(IGdsVertexStoreWriter storeWriter) : IParserConsumer
{
    public GdsLibrary Library => _library ?? throw new InvalidOperationException("Library has not been built yet.");
    
    private readonly GdsLibraryBuilder _builder = new(storeWriter);
    private GdsLibrary? _library;

    private CellId? _currentStructureId;
    private ShapeId? _currentShapeId;
    private StructureReferenceId? _currentStructureReferenceId;
    private ArrayReferenceId? _currentArrayReferenceId;
    
    public void OnBeginLibrary(in GdsLibraryInfo lib)
    {
        _builder.SetInfo(lib);
    }

    public void OnEndLibrary()
    {
        _library = _builder.Build();
    }

    public void OnBeginStructure(in GdsStructureInfo str)
    {
        _currentStructureId = _builder.AddStructure(str);
    }

    public void OnEndStructure()
    {
        if(!_currentStructureId.HasValue) throw new InvalidOperationException("No structure is currently being processed.");
        _currentStructureId = null;
    }

    public void OnBeginElement(GdsElementKind kind, in GdsElementCommon? common)
    {
        
    }

    public void OnProperty(short attr, string value)
    {
        var property = _currentShapeId.HasValue ? PropertyRecord.ForShape(attr, value, _currentShapeId.Value) 
            : _currentStructureReferenceId.HasValue ? PropertyRecord.ForStructureReference(attr, value, _currentStructureReferenceId.Value) 
            : _currentArrayReferenceId.HasValue ? PropertyRecord.ForArrayReference(attr, value, _currentArrayReferenceId.Value) 
            : throw new InvalidOperationException("No element is currently being processed.");
        _builder.AddElementProperty(property);
    }

    public void OnEndElement()
    {
        _currentShapeId = null;
        _currentStructureReferenceId = null;
        _currentArrayReferenceId = null;
    }

    public void OnBoundary(short layer, short dataType, ReadOnlySpan<GdsPoint> points)
    {
        if(!_currentStructureId.HasValue) throw new InvalidOperationException("No structure is currently being processed."); 
        _currentShapeId = _builder.AddBoundary(_currentStructureId.Value, layer, dataType, points);
    }

    public void OnPath(short layer, short dataType, GdsPathType? pathType, int? width, ReadOnlySpan<GdsPoint> points)
    {
        if(!width.HasValue) throw new InvalidOperationException("Path width must be specified.");
        if(!_currentStructureId.HasValue) throw new InvalidOperationException("No structure is currently being processed.");
        _currentShapeId = _builder.AddPath(_currentStructureId.Value, layer, dataType, points, width.Value);
    }

    public void OnBox(short layer, short boxType, ReadOnlySpan<GdsPoint> points)
    {
        if(!_currentStructureId.HasValue) throw new InvalidOperationException("No structure is currently being processed.");
        if(points.Length != 5) throw new InvalidOperationException("BOX must have exactly 5 points.");
        _currentShapeId = _builder.AddBox(_currentStructureId.Value, layer, boxType, points);
    }

    public void OnNode(short layer, short nodeType, ReadOnlySpan<GdsPoint> points)
    {
        if(!_currentStructureId.HasValue) throw new InvalidOperationException("No structure is currently being processed.");
        _currentShapeId = _builder.AddNode(_currentStructureId.Value, layer, nodeType, points);
    }

    public void OnSref(string structureName, GdsStransInfo? strans, ReadOnlySpan<GdsPoint> points)
    {
        if(!_currentStructureId.HasValue) throw new InvalidOperationException("No structure is currently being processed.");
        if (points.Length != 1) throw new InvalidOperationException("SREF must have exactly one origin point.");
        
        var origin = points[0];
        strans ??= new GdsStransInfo();
        var transform = new GdsTransform(strans.Value, origin);

        _currentStructureReferenceId = _builder.AddStructureReference(_currentStructureId.Value, structureName, transform);
    }

    public void OnAref(string structureName, GdsStransInfo? strans, short cols, short rows, ReadOnlySpan<GdsPoint> points)
    {
        if(!_currentStructureId.HasValue) throw new InvalidOperationException("No structure is currently being processed.");
        if (points.Length != 3) throw new InvalidOperationException("AREF must have exactly three points: origin, row vector, column vector.");
        
        var origin = points[0];
        var rowVector = new GdsPoint(points[1].X - origin.X, points[1].Y - origin.Y);
        var columnVector = new GdsPoint(points[2].X - origin.X, points[2].Y - origin.Y);
        
        strans ??= new GdsStransInfo();
        var transform = new GdsTransform(strans.Value, origin);
        
        _currentArrayReferenceId = _builder.AddArrayReference(_currentStructureId.Value, structureName, transform, cols, rows, rowVector, columnVector);
    }

    public void OnText(short layer, short textType, PresentationInfo? presentation, GdsPathType? pathType, int? width, GdsStransInfo? strans, ReadOnlySpan<GdsPoint> points, string text)
    {
        if(points.Length != 1) throw new InvalidOperationException("TEXT must have exactly one origin point.");
        if(!_currentStructureId.HasValue) throw new InvalidOperationException("No structure is currently being processed.");
        
        var origin = points[0];
        strans ??= new GdsStransInfo();
        var transform = new GdsTransform(strans.Value, origin);
        
        _currentShapeId = _builder.AddText(
            _currentStructureId.Value,
            layer,
            textType,
            text,
            presentation,
            pathType,
            width,
            transform
        );
    }
}