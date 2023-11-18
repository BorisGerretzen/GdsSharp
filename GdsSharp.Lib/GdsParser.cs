using GdsSharp.Lib.Abstractions;
using GdsSharp.Lib.NonTerminals;
using GdsSharp.Lib.NonTerminals.Elements;
using GdsSharp.Lib.NonTerminals.Enum;
using GdsSharp.Lib.Terminals.Abstractions;
using GdsSharp.Lib.Terminals.Records;

namespace GdsSharp.Lib;

public class GdsParser
{
    private readonly Queue<IGdsRecord> _queue;
    private int _tokenOffset = 0;
    
    public GdsParser(IEnumerable<IGdsRecord> tokens)
    {
        _queue = new Queue<IGdsRecord>(tokens);
    }

    public GdsFile Parse()
    {
        var header = Expect<GdsRecordHeader>();
        var bgnLib = Expect<GdsRecordBgnLib>();
        var libName = Expect<GdsRecordLibName>();
        var refLibs = ExpectOrDefault<GdsRecordRefLibs>();
        var fonts = ExpectOrDefault<GdsRecordFonts>();
        // var attrTable = ExpectOrDefault<GdsRecordAttrTable>();
        var generations = ExpectOrDefault<GdsRecordGenerations>();
        var format = IsNext<GdsRecordFormat>() ? Expect<GdsRecordFormat>() : null;
        var units = Expect<GdsRecordUnits>();
        
        var structures = new List<GdsStructure>();
        while (_queue.Peek() is not GdsRecordNoData { Type: GdsRecordNoDataType.EndLib })
        {
            structures.Add(ParseGdsStructure());
        }

        var file = new GdsFile
        {
            Version = header.Value,
            LibraryName = libName.Value,
            LastModificationTime = new DateTime(bgnLib.LastModificationTimeYear, bgnLib.LastModificationTimeMonth, bgnLib.LastModificationTimeDay,
                bgnLib.LastModificationTimeHour, bgnLib.LastModificationTimeMinute, bgnLib.LastModificationTimeSecond),
            LastAccessTime = new DateTime(bgnLib.LastAccessTimeYear, bgnLib.LastAccessTimeMonth, bgnLib.LastAccessTimeDay, bgnLib.LastAccessTimeHour,
                bgnLib.LastAccessTimeMinute, bgnLib.LastAccessTimeSecond),
            PhysicalUnits = units.PhysicalUnits,
            UserUnits = units.UserUnits,
        };
        
        if(refLibs is not null)
            file.ReferencedLibraries = refLibs.Libraries;
        
        if(fonts is not null)
            file.Fonts = fonts.Fonts;
        
        if(generations is not null)
            file.Generations = generations.Value;
        
        if(format is not null)
            file.FormatType = (GdsFormatType)format.Value;

        file.Structures = structures;
        
        return file;
    }

    private T Peek<T>()
        where T : IGdsRecord
    {
        var token = _queue.Peek();
        if (token is not T record) throw new ParseException(typeof(T), token, _tokenOffset);

        return record;
    }
    
    private T Expect<T>()
        where T : IGdsRecord
    {
        var token = _queue.Dequeue();
        _tokenOffset++;
        if (token is not T record) throw new ParseException(typeof(T), token, _tokenOffset);

        return record;
    }

    private T? ExpectOrDefault<T>()
        where T : IGdsRecord
    {
        var token = _queue.Peek();
        if (token is not T) return default;
        _tokenOffset++;
        return (T)_queue.Dequeue();
    }

    private bool IsNext<T>()
        where T : IGdsRecord
    {
        return _queue.Peek() is T;
    }

    private void ParseElement(IGdsElement element, GdsRecordElFlags? flags, GdsRecordPlex? plex)
    {
        if (flags is not null)
        {
            element.ExternalData = flags.ExternalData;
            element.TemplateData = flags.TemplateData;
        }

        if (plex is not null)
        {
            element.PlexNumber = plex.Value;
        }
    }

    private void GetNoData(GdsRecordNoDataType type)
    {
        var token = Expect<GdsRecordNoData>();
        if (token.Type != type) throw new ParseException(type, token.Type, _tokenOffset);
    }

    private GdsProperty ParseGdsProperty()
    {
        return new GdsProperty
        {
            Attribute = Expect<GdsRecordPropAttr>().Value,
            Value = Expect<GdsRecordPropValue>().Value
        };
    }

    private GdsStrans ParseTransformation()
    {
        var strans = Expect<GdsRecordSTrans>();

        var returnObj = new GdsStrans
        {
            Reflection = strans.Reflection,
            AbsoluteAngle = strans.AbsoluteAngle,
            AbsoluteMagnification = strans.AbsoluteMagnification,
        };

        if (ExpectOrDefault<GdsRecordMag>() is { } mag)
            returnObj.Magnification = mag.Value;

        if (ExpectOrDefault<GdsRecordAngle>() is { } angle)
            returnObj.Angle = angle.Value;

        return returnObj;
    }

    private GdsBoxElement ParseGdsBoxElement()
    {
        GetNoData(GdsRecordNoDataType.Box);
        var flags = ExpectOrDefault<GdsRecordElFlags>();
        var plex = ExpectOrDefault<GdsRecordPlex>();
        var layer = Expect<GdsRecordLayer>();
        var boxType = Expect<GdsRecordBoxType>();
        var xy = Expect<GdsRecordXy>();
        var elem = new GdsBoxElement
        {
            BoxType = boxType.Value,
            Layer = layer.Value,
            Points = xy.Coordinates.AsGdsPoints(),
        };

        ParseElement(elem, flags, plex);

        return elem;
    }

    private GdsNodeElement ParseGdsNodeElement()
    {
        GetNoData(GdsRecordNoDataType.Node);
        var flags = ExpectOrDefault<GdsRecordElFlags>();
        var plex = ExpectOrDefault<GdsRecordPlex>();
        var layer = Expect<GdsRecordLayer>();
        var nodeType = Expect<GdsRecordNodeType>();
        var xy = Expect<GdsRecordXy>();

        var elem = new GdsNodeElement
        {
            Layer = layer.Value,
            Points = xy.Coordinates.AsGdsPoints(),
            NodeType = nodeType.Value
        };

        ParseElement(elem, flags, plex);

        return elem;
    }

    private GdsTextElement ParseGdsTextElement()
    {
        GetNoData(GdsRecordNoDataType.Text);
        var flags = ExpectOrDefault<GdsRecordElFlags>();
        var plex = ExpectOrDefault<GdsRecordPlex>();
        var layer = Expect<GdsRecordLayer>();
        var textType = Expect<GdsRecordTextType>();
        var presentation = ExpectOrDefault<GdsRecordPresentation>();
        var pathType = ExpectOrDefault<GdsRecordPathType>();
        var width = ExpectOrDefault<GdsRecordWidth>();
        var transformation = IsNext<GdsRecordSTrans>() ? ParseTransformation() : null;
        var xy = Expect<GdsRecordXy>();
        var str = Expect<GdsRecordString>();

        var elem = new GdsTextElement
        {
            Text = str.Value,
            Layer = layer.Value,
            TextType = textType.Value,
            Points = xy.Coordinates.AsGdsPoints(),
        };

        ParseElement(elem, flags, plex);

        if (presentation is not null)
        {
            elem.HorizontalJustification = (GdsHorizontalJustification)presentation.HorizontalPresentation;
            elem.VerticalJustification = (GdsVerticalJustification)presentation.VerticalPresentation;
            elem.Font = (GdsFont)presentation.FontNumber;
        }

        if (pathType is not null)
        {
            elem.PathType = (GdsPathType)pathType.Value;
        }

        if (width is not null)
        {
            elem.PathType = (GdsPathType)width.Value;
        }

        if (transformation is not null)
        {
            elem.Transformation = transformation;
        }

        return elem;
    }

    private GdsArrayReferenceElement ParseGdsArrayReferenceElement()
    {
        GetNoData(GdsRecordNoDataType.Aref);
        var flags = ExpectOrDefault<GdsRecordElFlags>();
        var plex = ExpectOrDefault<GdsRecordPlex>();
        var name = Expect<GdsRecordSName>();
        var transformation = IsNext<GdsRecordSTrans>() ? ParseTransformation() : null;
        var colRow = Expect<GdsRecordColRow>();
        var xy = Expect<GdsRecordXy>();

        var elem = new GdsArrayReferenceElement
        {
            Columns = colRow.NumCols,
            Rows = colRow.NumRows,
            Points = xy.Coordinates.AsGdsPoints(),
            StructureName = name.Value
        };

        ParseElement(elem, flags, plex);

        if (transformation is not null)
        {
            elem.Transformation = transformation;
        }

        return elem;
    }

    private GdsStructureReferenceElement ParseGdsStructureReferenceElement()
    {
        GetNoData(GdsRecordNoDataType.Sref);
        var flags = ExpectOrDefault<GdsRecordElFlags>();
        var plex = ExpectOrDefault<GdsRecordPlex>();
        var name = Expect<GdsRecordSName>();
        var transformation = IsNext<GdsRecordSTrans>() ? ParseTransformation() : null;
        var xy = Expect<GdsRecordXy>();

        var elem = new GdsStructureReferenceElement
        {
            StructureName = name.Value,
            Points = xy.Coordinates.AsGdsPoints(),
        };

        ParseElement(elem, flags, plex);

        if (transformation is not null)
        {
            elem.Transformation = transformation;
        }

        return elem;
    }

    private GdsPathElement ParseGdsPathElement()
    {
        GetNoData(GdsRecordNoDataType.Path);
        var flags = ExpectOrDefault<GdsRecordElFlags>();
        var plex = ExpectOrDefault<GdsRecordPlex>();
        var layer = Expect<GdsRecordLayer>();
        var dataType = Expect<GdsRecordDataType>();
        var pathType = ExpectOrDefault<GdsRecordPathType>();
        var width = ExpectOrDefault<GdsRecordWidth>();
        var xy = Expect<GdsRecordXy>();

        var elem = new GdsPathElement
        {
            DataType = dataType.Value,
            Layer = layer.Value,
            Points = xy.Coordinates.AsGdsPoints(),
        };

        ParseElement(elem, flags, plex);

        if (pathType is not null)
        {
            elem.PathType = (GdsPathType)pathType.Value;
        }

        if (width is not null)
        {
            elem.PathType = (GdsPathType)width.Value;
        }

        return elem;
    }

    private GdsBoundaryElement ParseGdsBoundaryElement()
    {
        GetNoData(GdsRecordNoDataType.Boundary);
        var flags = ExpectOrDefault<GdsRecordElFlags>();
        var plex = ExpectOrDefault<GdsRecordPlex>();
        var layer = Expect<GdsRecordLayer>();
        var dataType = Expect<GdsRecordDataType>();
        var xy = Expect<GdsRecordXy>();

        var elem = new GdsBoundaryElement
        {
            DataType = dataType.Value,
            Layer = layer.Value,
            Points = xy.Coordinates.AsGdsPoints(),
        };

        ParseElement(elem, flags, plex);

        return elem;
    }

    private GdsElement ParseGdsElement()
    {
        var token = Peek<GdsRecordNoData>();

        IGdsElement elem = token.Type switch
        {
            GdsRecordNoDataType.Box => ParseGdsBoxElement(),
            GdsRecordNoDataType.Node => ParseGdsNodeElement(),
            GdsRecordNoDataType.Text => ParseGdsTextElement(),
            GdsRecordNoDataType.Aref => ParseGdsArrayReferenceElement(),
            GdsRecordNoDataType.Sref => ParseGdsStructureReferenceElement(),
            GdsRecordNoDataType.Path => ParseGdsPathElement(),
            GdsRecordNoDataType.Boundary => ParseGdsBoundaryElement(),
            _ => throw new ParseException(token.Type, _tokenOffset)
        };

        var properties = new List<GdsProperty>();
        while (_queue.Peek() is not GdsRecordNoData { Type: GdsRecordNoDataType.EndEl })
        {
            properties.Add(ParseGdsProperty());
        }

        GetNoData(GdsRecordNoDataType.EndEl);

        return new GdsElement
        {
            Element = elem,
            Properties = properties
        };
    }

    private GdsStructure ParseGdsStructure()
    {
        var bgnStr = Expect<GdsRecordBgnStr>();
        var name = Expect<GdsRecordStrName>();

        var elements = new List<GdsElement>();
        while (_queue.Peek() is not GdsRecordNoData { Type: GdsRecordNoDataType.EndStr })
        {
            elements.Add(ParseGdsElement());
        }
        GetNoData(GdsRecordNoDataType.EndStr);
        return new GdsStructure
        {
            Name = name.Value,
            CreationTime = new DateTime(bgnStr.CreationTimeYear, bgnStr.CreationTimeMonth, bgnStr.CreationTimeDay, bgnStr.CreationTimeHour, bgnStr.CreationTimeMinute, bgnStr.CreationTimeSecond),
            ModificationTime = new DateTime(bgnStr.LastModificationTimeYear, bgnStr.LastModificationTimeMonth, bgnStr.LastModificationTimeDay, bgnStr.LastModificationTimeHour, bgnStr.LastModificationTimeMinute, bgnStr.LastModificationTimeSecond),
            Elements = elements
        };
    }
}