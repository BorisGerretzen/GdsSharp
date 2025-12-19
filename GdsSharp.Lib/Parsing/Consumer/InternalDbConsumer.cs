using GdsSharp.Lib.InternalDb;
using GdsSharp.Lib.InternalDb.Builder;
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
    private int? _currentElementId;
    private GdsElementCommon? _currentElementCommon;
    
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
        _currentElementCommon = common;
    }

    public void OnProperty(short attr, string value)
    {
        if(!_currentElementId.HasValue) throw new InvalidOperationException("No element is currently being processed.");
        _builder.AddElementProperty(_currentElementId.Value, attr, value);
    }

    public void OnEndElement()
    {
        _currentElementId = null;
        _currentElementCommon = null;
    }

    public void OnBoundary(short layer, short dataType, ReadOnlySpan<GdsPoint> points)
    {
        if(!_currentStructureId.HasValue) throw new InvalidOperationException("No structure is currently being processed."); 
        if(!_currentElementCommon.HasValue) throw new InvalidOperationException("Element common data is missing.");
        _currentElementId = _builder.AddBoundary(_currentStructureId.Value, _currentElementCommon.Value, layer, dataType, points);
    }

    public void OnPath(short layer, short dataType, GdsPathType? pathType, int? width, ReadOnlySpan<GdsPoint> points)
    {
        if(!_currentStructureId.HasValue) throw new InvalidOperationException("No structure is currently being processed.");
        if(!_currentElementCommon.HasValue) throw new InvalidOperationException("Element common data is missing.");
        _currentElementId = _builder.AddPath(_currentStructureId.Value, _currentElementCommon.Value, layer, dataType, points, width, pathType);
    }

    public void OnBox(short layer, short boxType, ReadOnlySpan<GdsPoint> points)
    {
        if(!_currentStructureId.HasValue) throw new InvalidOperationException("No structure is currently being processed.");
        if(points.Length != 5) throw new InvalidOperationException("BOX must have exactly 5 points.");
        if(points[0] != points[4]) throw new InvalidOperationException("The first and last points of a BOX must be the same.");
        if(!_currentElementCommon.HasValue) throw new InvalidOperationException("Element common data is missing.");
        _currentElementId = _builder.AddBox(_currentStructureId.Value, _currentElementCommon.Value, layer, boxType, points);
    }

    public void OnNode(short layer, short nodeType, ReadOnlySpan<GdsPoint> points)
    {
        if(!_currentStructureId.HasValue) throw new InvalidOperationException("No structure is currently being processed.");
        if(!_currentElementCommon.HasValue) throw new InvalidOperationException("Element common data is missing.");
        _currentElementId = _builder.AddNode(_currentStructureId.Value, _currentElementCommon.Value, layer, nodeType, points);
    }

    public void OnSref(string structureName, GdsStransInfo? strans, ReadOnlySpan<GdsPoint> points)
    {
        if(!_currentStructureId.HasValue) throw new InvalidOperationException("No structure is currently being processed.");
        if (points.Length != 1) throw new InvalidOperationException("SREF must have exactly one origin point.");
        if(!_currentElementCommon.HasValue) throw new InvalidOperationException("Element common data is missing.");
        
        var origin = points[0];
        _currentElementId = _builder.AddStructureReference(_currentStructureId.Value, _currentElementCommon.Value, structureName, strans, origin);
    }

    public void OnAref(string structureName, GdsStransInfo? strans, short cols, short rows, ReadOnlySpan<GdsPoint> points)
    {
        if(!_currentStructureId.HasValue) throw new InvalidOperationException("No structure is currently being processed.");
        if (points.Length != 3) throw new InvalidOperationException("AREF must have exactly three points: origin, row vector, column vector.");
        if(!_currentElementCommon.HasValue) throw new InvalidOperationException("Element common data is missing.");
            
        var origin = points[0];
        var rowVector = points[1];
        var columnVector = points[2];
        
        _currentElementId = _builder.AddArrayReference(_currentStructureId.Value, _currentElementCommon.Value, structureName, strans, rows, cols, rowVector, columnVector, origin);
    }

    public void OnText(short layer, short textType, PresentationInfo? presentation, GdsPathType? pathType, int? width, GdsStransInfo? strans, ReadOnlySpan<GdsPoint> points, string text)
    {
        if(points.Length != 1) throw new InvalidOperationException("TEXT must have exactly one origin point.");
        if(!_currentStructureId.HasValue) throw new InvalidOperationException("No structure is currently being processed.");
        if(!_currentElementCommon.HasValue) throw new InvalidOperationException("Element common data is missing.");
        
        var origin = points[0];
        _currentElementId = _builder.AddText(_currentStructureId.Value, _currentElementCommon.Value, layer, textType, presentation, pathType, width, strans, origin, text);
    }
}