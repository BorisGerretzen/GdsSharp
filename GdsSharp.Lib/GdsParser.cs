using GdsSharp.Lib.NonTerminals;
using GdsSharp.Lib.NonTerminals.Abstractions;
using GdsSharp.Lib.NonTerminals.Elements;
using GdsSharp.Lib.NonTerminals.Enum;
using GdsSharp.Lib.Terminals.Abstractions;
using GdsSharp.Lib.Terminals.Records;

namespace GdsSharp.Lib;

public class GdsParser
{
    private readonly Queue<IGdsRecord> _queue;
    private int _tokenOffset;

    public GdsParser(IEnumerable<IGdsRecord> tokens)
    {
        _queue = new Queue<IGdsRecord>(tokens);
    }

    /// <summary>
    ///     Parses the GDSII token stream.
    /// </summary>
    /// <returns>Parsed file.</returns>
    /// <exception cref="ParseException">If the token stream is invalid.</exception>
    public GdsFile Parse()
    {
        var header = Get<GdsRecordHeader>();
        var bgnLib = Get<GdsRecordBgnLib>();
        var libName = Get<GdsRecordLibName>();
        var refLibs = GetOrDefault<GdsRecordRefLibs>();
        var fonts = GetOrDefault<GdsRecordFonts>();
        // var attrTable = ExpectOrDefault<GdsRecordAttrTable>();
        var generations = GetOrDefault<GdsRecordGenerations>();
        var format = IsNext<GdsRecordFormat>() ? Get<GdsRecordFormat>() : null;
        var units = Get<GdsRecordUnits>();

        var structures = new List<GdsStructure>();
        while (_queue.Peek() is not GdsRecordNoData { Type: GdsRecordNoDataType.EndLib }) structures.Add(ParseGdsStructure());

        var file = new GdsFile
        {
            Version = header.Value,
            LibraryName = libName.Value,
            LastModificationTime = new DateTime(bgnLib.LastModificationTimeYear, bgnLib.LastModificationTimeMonth, bgnLib.LastModificationTimeDay,
                bgnLib.LastModificationTimeHour, bgnLib.LastModificationTimeMinute, bgnLib.LastModificationTimeSecond),
            LastAccessTime = new DateTime(bgnLib.LastAccessTimeYear, bgnLib.LastAccessTimeMonth, bgnLib.LastAccessTimeDay, bgnLib.LastAccessTimeHour,
                bgnLib.LastAccessTimeMinute, bgnLib.LastAccessTimeSecond),
            PhysicalUnits = units.PhysicalUnits,
            UserUnits = units.UserUnits
        };

        if (refLibs is not null)
            file.ReferencedLibraries = refLibs.Libraries;

        if (fonts is not null)
            file.Fonts = fonts.Fonts;

        if (generations is not null)
            file.Generations = generations.Value;

        if (format is not null)
            file.FormatType = (GdsFormatType)format.Value;

        file.Structures = structures;

        return file;
    }

    #region Helpers

    /// <summary>
    ///     Peeks the next token and throws a <see cref="ParseException" /> if it is not of type <typeparamref name="T" />.
    /// </summary>
    /// <typeparam name="T">Expected token type.</typeparam>
    /// <returns>Peeked token of type <typeparamref name="T" />.</returns>
    /// <exception cref="ParseException">If peeked token is not of expected type.</exception>
    private T Peek<T>()
        where T : IGdsRecord
    {
        var token = _queue.Peek();
        if (token is not T record) throw new ParseException(typeof(T), token, _tokenOffset);

        return record;
    }

    /// <summary>
    ///     Gets the next token and throws a <see cref="ParseException" /> if it is not of type <typeparamref name="T" />.
    /// </summary>
    /// <typeparam name="T">Expected token type.</typeparam>
    /// <returns>Next token of type <typeparamref name="T" />.</returns>
    /// <exception cref="ParseException">If next token is not of expected type.</exception>
    private T Get<T>()
        where T : IGdsRecord
    {
        var token = _queue.Dequeue();
        _tokenOffset++;
        if (token is not T record) throw new ParseException(typeof(T), token, _tokenOffset);

        return record;
    }

    /// <summary>
    ///     Peeks the next token and dequeues it if it is of type <typeparamref name="T" />.
    ///     If the next token is not of type <typeparamref name="T" />, <see langword="null" /> is returned.
    /// </summary>
    /// <typeparam name="T">Expected token type.</typeparam>
    /// <returns>
    ///     Next token of type <typeparamref name="T" />, or <see langword="null" /> if next token is not of expected
    ///     type.
    /// </returns>
    private T? GetOrDefault<T>()
        where T : IGdsRecord
    {
        if (!IsNext<T>()) return default;
        _tokenOffset++;
        return (T)_queue.Dequeue();
    }

    /// <summary>
    ///     Checks if the next token is of type <typeparamref name="T" />.
    /// </summary>
    /// <typeparam name="T">Expected token type.</typeparam>
    /// <returns>True if next token is of type <typeparamref name="T" />, false otherwise.</returns>
    private bool IsNext<T>()
        where T : IGdsRecord
    {
        return _queue.Peek() is T;
    }

    /// <summary>
    ///     Gets the next token of type <see cref="GdsRecordNoData" /> and throws a <see cref="ParseException" /> if it is not
    ///     of type <paramref name="type" />.
    /// </summary>
    /// <param name="type">Expected <see cref="GdsRecordNoDataType" />.</param>
    /// <exception cref="ParseException">If next token is not of expected type.</exception>
    private void GetNoData(GdsRecordNoDataType type)
    {
        var token = Get<GdsRecordNoData>();
        if (token.Type != type) throw new ParseException(type, token.Type, _tokenOffset);
    }

    /// <summary>
    ///     Fills the common properties of an <see cref="IGdsElement" /> from the given <paramref name="flags" /> and
    ///     <paramref name="plex" />.
    /// </summary>
    /// <param name="element">Element to fill.</param>
    /// <param name="flags">Flags to use.</param>
    /// <param name="plex">Plex to use.</param>
    private static void FillElement(IGdsElement element, GdsRecordElFlags? flags, GdsRecordPlex? plex)
    {
        if (flags is not null)
        {
            element.ExternalData = flags.ExternalData;
            element.TemplateData = flags.TemplateData;
        }

        if (plex is not null) element.PlexNumber = plex.Value;
    }

    #endregion

    #region Parsers

    private GdsProperty ParseGdsProperty()
    {
        return new GdsProperty
        {
            Attribute = Get<GdsRecordPropAttr>().Value,
            Value = Get<GdsRecordPropValue>().Value
        };
    }

    private GdsStrans ParseTransformation()
    {
        var strans = Get<GdsRecordSTrans>();

        var returnObj = new GdsStrans
        {
            Reflection = strans.Reflection,
            AbsoluteAngle = strans.AbsoluteAngle,
            AbsoluteMagnification = strans.AbsoluteMagnification
        };

        if (GetOrDefault<GdsRecordMag>() is { } mag)
            returnObj.Magnification = mag.Value;

        if (GetOrDefault<GdsRecordAngle>() is { } angle)
            returnObj.Angle = angle.Value;

        return returnObj;
    }

    private GdsBoxElement ParseGdsBoxElement()
    {
        GetNoData(GdsRecordNoDataType.Box);
        var flags = GetOrDefault<GdsRecordElFlags>();
        var plex = GetOrDefault<GdsRecordPlex>();
        var layer = Get<GdsRecordLayer>();
        var boxType = Get<GdsRecordBoxType>();
        var xy = Get<GdsRecordXy>();
        var elem = new GdsBoxElement
        {
            BoxType = boxType.Value,
            Layer = layer.Value,
            Points = xy.Coordinates.AsGdsPoints()
        };

        FillElement(elem, flags, plex);

        return elem;
    }

    private GdsNodeElement ParseGdsNodeElement()
    {
        GetNoData(GdsRecordNoDataType.Node);
        var flags = GetOrDefault<GdsRecordElFlags>();
        var plex = GetOrDefault<GdsRecordPlex>();
        var layer = Get<GdsRecordLayer>();
        var nodeType = Get<GdsRecordNodeType>();
        var xy = Get<GdsRecordXy>();

        var elem = new GdsNodeElement
        {
            Layer = layer.Value,
            Points = xy.Coordinates.AsGdsPoints(),
            NodeType = nodeType.Value
        };

        FillElement(elem, flags, plex);

        return elem;
    }

    private GdsTextElement ParseGdsTextElement()
    {
        GetNoData(GdsRecordNoDataType.Text);
        var flags = GetOrDefault<GdsRecordElFlags>();
        var plex = GetOrDefault<GdsRecordPlex>();
        var layer = Get<GdsRecordLayer>();
        var textType = Get<GdsRecordTextType>();
        var presentation = GetOrDefault<GdsRecordPresentation>();
        var pathType = GetOrDefault<GdsRecordPathType>();
        var width = GetOrDefault<GdsRecordWidth>();
        var transformation = IsNext<GdsRecordSTrans>() ? ParseTransformation() : null;
        var xy = Get<GdsRecordXy>();
        var str = Get<GdsRecordString>();

        var elem = new GdsTextElement
        {
            Text = str.Value,
            Layer = layer.Value,
            TextType = textType.Value,
            Points = xy.Coordinates.AsGdsPoints()
        };

        FillElement(elem, flags, plex);

        if (presentation is not null)
        {
            elem.HorizontalJustification = (GdsHorizontalJustification)presentation.HorizontalPresentation;
            elem.VerticalJustification = (GdsVerticalJustification)presentation.VerticalPresentation;
            elem.Font = (GdsFont)presentation.FontNumber;
        }

        if (pathType is not null) elem.PathType = (GdsPathType)pathType.Value;

        if (width is not null) elem.Width = width.Value;

        if (transformation is not null) elem.Transformation = transformation;

        return elem;
    }

    private GdsArrayReferenceElement ParseGdsArrayReferenceElement()
    {
        GetNoData(GdsRecordNoDataType.Aref);
        var flags = GetOrDefault<GdsRecordElFlags>();
        var plex = GetOrDefault<GdsRecordPlex>();
        var name = Get<GdsRecordSName>();
        var transformation = IsNext<GdsRecordSTrans>() ? ParseTransformation() : null;
        var colRow = Get<GdsRecordColRow>();
        var xy = Get<GdsRecordXy>();

        var elem = new GdsArrayReferenceElement
        {
            Columns = colRow.NumCols,
            Rows = colRow.NumRows,
            Points = xy.Coordinates.AsGdsPoints(),
            StructureName = name.Value
        };

        FillElement(elem, flags, plex);

        if (transformation is not null) elem.Transformation = transformation;

        return elem;
    }

    private GdsStructureReferenceElement ParseGdsStructureReferenceElement()
    {
        GetNoData(GdsRecordNoDataType.Sref);
        var flags = GetOrDefault<GdsRecordElFlags>();
        var plex = GetOrDefault<GdsRecordPlex>();
        var name = Get<GdsRecordSName>();
        var transformation = IsNext<GdsRecordSTrans>() ? ParseTransformation() : null;
        var xy = Get<GdsRecordXy>();

        var elem = new GdsStructureReferenceElement
        {
            StructureName = name.Value,
            Points = xy.Coordinates.AsGdsPoints()
        };

        FillElement(elem, flags, plex);

        if (transformation is not null) elem.Transformation = transformation;

        return elem;
    }

    private GdsPathElement ParseGdsPathElement()
    {
        GetNoData(GdsRecordNoDataType.Path);
        var flags = GetOrDefault<GdsRecordElFlags>();
        var plex = GetOrDefault<GdsRecordPlex>();
        var layer = Get<GdsRecordLayer>();
        var dataType = Get<GdsRecordDataType>();
        var pathType = GetOrDefault<GdsRecordPathType>();
        var width = GetOrDefault<GdsRecordWidth>();
        var xy = Get<GdsRecordXy>();

        var elem = new GdsPathElement
        {
            DataType = dataType.Value,
            Layer = layer.Value,
            Points = xy.Coordinates.AsGdsPoints()
        };

        FillElement(elem, flags, plex);

        if (pathType is not null) elem.PathType = (GdsPathType)pathType.Value;

        if (width is not null) elem.Width = width.Value;

        return elem;
    }

    private GdsBoundaryElement ParseGdsBoundaryElement()
    {
        GetNoData(GdsRecordNoDataType.Boundary);
        var flags = GetOrDefault<GdsRecordElFlags>();
        var plex = GetOrDefault<GdsRecordPlex>();
        var layer = Get<GdsRecordLayer>();
        var dataType = Get<GdsRecordDataType>();
        var xy = Get<GdsRecordXy>();

        var elem = new GdsBoundaryElement
        {
            DataType = dataType.Value,
            Layer = layer.Value,
            Points = xy.Coordinates.AsGdsPoints()
        };

        FillElement(elem, flags, plex);

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
        while (_queue.Peek() is not GdsRecordNoData { Type: GdsRecordNoDataType.EndEl }) properties.Add(ParseGdsProperty());

        GetNoData(GdsRecordNoDataType.EndEl);

        return new GdsElement
        {
            Element = elem,
            Properties = properties
        };
    }

    private GdsStructure ParseGdsStructure()
    {
        var bgnStr = Get<GdsRecordBgnStr>();
        var name = Get<GdsRecordStrName>();

        var elements = new List<GdsElement>();
        while (_queue.Peek() is not GdsRecordNoData { Type: GdsRecordNoDataType.EndStr }) elements.Add(ParseGdsElement());

        GetNoData(GdsRecordNoDataType.EndStr);
        return new GdsStructure
        {
            Name = name.Value,
            CreationTime = new DateTime(bgnStr.CreationTimeYear, bgnStr.CreationTimeMonth, bgnStr.CreationTimeDay, bgnStr.CreationTimeHour,
                bgnStr.CreationTimeMinute, bgnStr.CreationTimeSecond),
            ModificationTime = new DateTime(bgnStr.LastModificationTimeYear, bgnStr.LastModificationTimeMonth, bgnStr.LastModificationTimeDay,
                bgnStr.LastModificationTimeHour, bgnStr.LastModificationTimeMinute, bgnStr.LastModificationTimeSecond),
            Elements = elements
        };
    }

    #endregion
}