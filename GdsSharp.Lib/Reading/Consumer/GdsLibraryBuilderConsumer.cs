using GdsSharp.Lib.Library;
using GdsSharp.Lib.Library.Builder;
using GdsSharp.Lib.Library.VertexStore;
using GdsSharp.Lib.Reading.Enum;
using GdsSharp.Lib.Reading.Models;

namespace GdsSharp.Lib.Reading.Consumer;

public class GdsLibraryBuilderConsumer(IGdsVertexStore storeWriter) : IParserConsumer
{
    private readonly GdsLibraryBuilder _builder = new(storeWriter);
    private GdsElementCommon? _currentElementCommon;
    private int? _currentElementId;

    private CellId? _currentStructureId;
    private GdsLibrary? _library;
    public GdsLibrary Library => _library ?? throw new InvalidOperationException("Library has not been built yet.");

    public void OnBeginLibrary(in GdsLibraryInfo lib)
    {
        _builder.Info = lib;
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
        if (!_currentStructureId.HasValue) throw new InvalidOperationException("No structure is currently being processed.");
        _currentStructureId = null;
    }

    public void OnBeginElement(GdsElementKind kind, in GdsElementCommon? common)
    {
        _currentElementCommon = common;
    }

    public void OnProperty(short attr, string value)
    {
        if (!_currentElementId.HasValue) throw new InvalidOperationException("No element is currently being processed.");
        _builder.AddElementProperty(_currentElementId.Value, attr, value);
    }

    public void OnEndElement()
    {
        _currentElementId = null;
        _currentElementCommon = null;
    }

    public void OnBoundary(short layer, short dataType, ReadOnlySpan<GdsPoint> points)
    {
        if (!_currentStructureId.HasValue) throw new InvalidOperationException("No structure is currently being processed.");
        _currentElementId = _builder.AddBoundary(_currentElementCommon, layer, dataType, points);
    }

    public void OnPath(short layer, short dataType, GdsPathType? pathType, int? width, ReadOnlySpan<GdsPoint> points)
    {
        if (!_currentStructureId.HasValue) throw new InvalidOperationException("No structure is currently being processed.");
        _currentElementId = _builder.AddPath(_currentElementCommon, layer, dataType, points, width, pathType);
    }

    public void OnBox(short layer, short boxType, ReadOnlySpan<GdsPoint> points)
    {
        if (!_currentStructureId.HasValue) throw new InvalidOperationException("No structure is currently being processed.");
        if (points.Length != 5) throw new InvalidOperationException("BOX must have exactly 5 points.");
        if (points[0] != points[4]) throw new InvalidOperationException("The first and last points of a BOX must be the same.");
        _currentElementId = _builder.AddBox(_currentElementCommon, layer, boxType, points);
    }

    public void OnNode(short layer, short nodeType, ReadOnlySpan<GdsPoint> points)
    {
        if (!_currentStructureId.HasValue) throw new InvalidOperationException("No structure is currently being processed.");
        _currentElementId = _builder.AddNode(_currentElementCommon, layer, nodeType, points);
    }

    public void OnSref(string structureName, GdsStransInfo? strans, ReadOnlySpan<GdsPoint> points)
    {
        if (!_currentStructureId.HasValue) throw new InvalidOperationException("No structure is currently being processed.");
        if (points.Length != 1) throw new InvalidOperationException("SREF must have exactly one origin point.");

        var origin = points[0];
        _currentElementId = _builder.AddStructureReference(_currentElementCommon, structureName, strans, origin);
    }

    public void OnAref(string structureName, GdsStransInfo? strans, short cols, short rows, ReadOnlySpan<GdsPoint> points)
    {
        if (!_currentStructureId.HasValue) throw new InvalidOperationException("No structure is currently being processed.");
        if (points.Length != 3) throw new InvalidOperationException("AREF must have exactly three points: origin, column vector, row vector.");

        var origin = points[0];
        var columnVector = points[1];
        var rowVector = points[2];

        _currentElementId = _builder.AddArrayReference(_currentElementCommon, structureName, strans, rows, cols, rowVector, columnVector, origin);
    }

    public void OnText(short layer, short textType, PresentationInfo? presentation, GdsPathType? pathType, int? width, GdsStransInfo? strans, ReadOnlySpan<GdsPoint> points,
        string text)
    {
        if (points.Length != 1) throw new InvalidOperationException("TEXT must have exactly one origin point.");
        if (!_currentStructureId.HasValue) throw new InvalidOperationException("No structure is currently being processed.");

        var origin = points[0];
        _currentElementId = _builder.AddText(_currentElementCommon, layer, textType, presentation, pathType, width, strans, origin, text);
    }
}