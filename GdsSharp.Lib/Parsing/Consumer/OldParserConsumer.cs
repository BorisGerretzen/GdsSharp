using GdsSharp.Lib.Old;
using GdsSharp.Lib.Old.NonTerminals;
using GdsSharp.Lib.Old.NonTerminals.Abstractions;
using GdsSharp.Lib.Old.NonTerminals.Elements;
using GdsSharp.Lib.Old.NonTerminals.Enum;
using GdsSharp.Lib.Parsing.Models;

namespace GdsSharp.Lib.Parsing.Consumer;

/// <summary>
/// Reconstructs the same model graph as the original GdsParser.Parse().
/// Intended as a baseline adapter from the new event-based parser.
/// </summary>
public sealed class OldParserConsumer : IParserConsumer
{
    public GdsFile File => _file ?? throw new InvalidOperationException("Parse has not produced a file yet.");

    private GdsFile? _file;
    private readonly List<GdsStructure> _structures = new();

    private GdsStructure? _currentStructure;

    private IGdsElement? _currentElement;
    private GdsElementKind? _currentKind;
    private List<GdsProperty>? _currentProperties;
    private GdsElementCommon? _currentCommon;

    public void OnBeginLibrary(in GdsLibraryInfo lib)
    {
        _file = new GdsFile
        {
            Version = lib.Version,
            LibraryName = lib.Name,
            LastModificationTime = lib.ModificationTime,
            LastAccessTime = lib.AccessTime,
            PhysicalUnits = lib.PhysicalUnits,
            UserUnits = lib.UserUnits,
            Structures = _structures,
            ReferencedLibraries = lib.ReferencedLibraries.ToList(),
            Fonts = lib.Fonts.ToList(),
            Generations = lib.Generations ?? 3,
            FormatType = lib.FormatType
        };
    }

    public void OnEndLibrary()
    {
        if (_currentStructure is not null)
            throw new InvalidOperationException("ENDLIB while inside a structure.");
        if (_currentElement is not null)
            throw new InvalidOperationException("ENDLIB while inside an element.");
    }

    public void OnBeginStructure(in GdsStructureInfo str)
    {
        if (_file is null) throw new InvalidOperationException("BeginStructure before BeginLibrary.");
        if (_currentStructure is not null) throw new InvalidOperationException("Nested structures are not allowed.");

        _currentStructure = new GdsStructure
        {
            Name = str.Name,
            CreationTime = str.CreationTime,
            ModificationTime = str.ModificationTime,
            Elements = new List<GdsElement>()
        };

        _structures.Add(_currentStructure);
    }

    public void OnEndStructure()
    {
        if (_currentStructure is null) throw new InvalidOperationException("EndStructure without BeginStructure.");
        if (_currentElement is not null) throw new InvalidOperationException("ENDSTR while inside an element.");

        _currentStructure = null;
    }

    public void OnBeginElement(GdsElementKind kind, in GdsElementCommon? common)
    {
        if (_currentStructure is null) throw new InvalidOperationException("BeginElement outside of a structure.");
        if (_currentElement is not null) throw new InvalidOperationException("Nested elements are not allowed.");

        _currentKind = kind;
        _currentProperties = [];
        _currentCommon = common;
    }

    public void OnProperty(short attr, string value)
    {
        if (_currentProperties is null) throw new InvalidOperationException("Property outside of element.");
        _currentProperties.Add(new GdsProperty { Attribute = attr, Value = value });
    }

    public void OnEndElement()
    {
        if (_currentStructure is null) throw new InvalidOperationException("EndElement outside of a structure.");
        if (_currentElement is null || _currentProperties is null) throw new InvalidOperationException("EndElement without BeginElement.");

        if (_currentCommon.HasValue)
        {
            var v = _currentCommon.Value;
            _currentElement.ExternalData = v.ExternalData ?? false;
            _currentElement.TemplateData = v.TemplateData ?? false;
            _currentElement.PlexNumber = v.PlexNumber ?? 0;
        }
        
        var wrapped = new GdsElement
        {
            Element = _currentElement,
            Properties = _currentProperties
        };

        if (_currentStructure.Elements is List<GdsElement> list)
            list.Add(wrapped);
        else
        {
            var newList = _currentStructure.Elements.ToList();
            newList.Add(wrapped);
            _currentStructure.Elements = newList;
        }

        _currentElement = null;
        _currentProperties = null;
        _currentKind = null;
    }
    
    public void OnBoundary(short layer, short dataType, ReadOnlySpan<GdsPoint> points)
    {
        RequireCurrent(GdsElementKind.Boundary);
        var arr = points.ToArray();
        var e = new GdsBoundaryElement
        {
            Layer = layer,
            DataType = dataType,
            Points = arr,
            NumPoints = arr.Length,
        };
        
        _currentElement = e;
    }

    public void OnPath(short layer, short dataType, GdsPathType? pathType, int? width, ReadOnlySpan<GdsPoint> points)
    {
        RequireCurrent(GdsElementKind.Path);
        var pts = points.ToArray().ToList();

        var e = new GdsPathElement
        {
            Points = pts,
            DataType = dataType,
            Layer = layer,
        };
        
        if (pathType is not null) e.PathType = pathType.Value;
        if (width is not null) e.Width = width.Value;
        
        _currentElement = e;
    }

    public void OnBox(short layer, short boxType, ReadOnlySpan<GdsPoint> points)
    {
        RequireCurrent(GdsElementKind.Box);
        var e = new GdsBoxElement
        {
            BoxType = boxType,
            Points = points.ToArray().ToList(),
            Layer = layer
        };
        
        _currentElement = e;
    }

    public void OnNode(short layer, short nodeType, ReadOnlySpan<GdsPoint> points)
    {
        RequireCurrent(GdsElementKind.Node);
        var e = new GdsNodeElement
        {
            Layer = layer,
            NodeType = nodeType,
            Points = points.ToArray().ToList()
        };
        
        _currentElement = e;
    }

    public void OnSref(string structureName, GdsStransInfo? strans, ReadOnlySpan<GdsPoint> points)
    {
        RequireCurrent(GdsElementKind.StructureReference);
        var e = new GdsStructureReferenceElement
        {
            StructureName = structureName,
            Transformation = ConvertStrans(strans) ?? new GdsStrans(),
            Points = points.ToArray().ToList(),
        };
        
        _currentElement = e;
    }

    public void OnAref(string structureName, GdsStransInfo? strans, short cols, short rows, ReadOnlySpan<GdsPoint> points)
    {
        RequireCurrent(GdsElementKind.ArrayReference);
        var e = new GdsArrayReferenceElement
        {
            StructureName = structureName,
            Columns = cols,
            Rows = rows,
            Transformation = ConvertStrans(strans) ?? new GdsStrans(),
            Points = points.ToArray().ToList(),
        };
        
        _currentElement = e;
    }

    public void OnText(short layer, short textType, PresentationInfo? presentation, GdsPathType? pathType, int? width,
        GdsStransInfo? strans, ReadOnlySpan<GdsPoint> points, string text)
    {
        RequireCurrent(GdsElementKind.Text);
        var e = new GdsTextElement
        {
            Text = text,
            TextType = textType,
            Layer = layer,
            Transformation =  ConvertStrans(strans) ?? new GdsStrans(),
            Points = points.ToArray().ToList(),
        };
        
        if (presentation is not null)
        {
            var p = presentation.Value;
            e.HorizontalJustification = (GdsHorizontalJustification)p.HorizontalJustification;
            e.VerticalJustification = (GdsVerticalJustification)p.VerticalJustification;
            e.Font = (GdsFont)p.Font;
        }

        if (pathType is not null) e.PathType = pathType.Value;
        if (width is not null) e.Width = width.Value;
        
        _currentElement = e;
    }
    
    private static GdsStrans? ConvertStrans(GdsStransInfo? strans)
    {
        if (strans is null) return null;

        var s = strans.Value;
        
        return new GdsStrans
        {
            Angle = s.Angle ?? 0,
            Magnification = s.Magnification ?? 1,
            AbsoluteAngle = s.AbsoluteAngle,
            AbsoluteMagnification = s.AbsoluteMagnification,
            Reflection = s.Reflection
        };
    }
    
    private void RequireCurrent(GdsElementKind expectedKind)
    {
        if (_currentKind is null)
            throw new InvalidOperationException("Element callback outside of BeginElement/EndElement.");

        if (_currentKind.Value != expectedKind)
            throw new InvalidOperationException($"Got {expectedKind} payload, but current element is {_currentKind.Value}.");
    }
}