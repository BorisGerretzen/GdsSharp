using GdsSharp.Lib.Lexing;
using GdsSharp.Lib.NonTerminals;
using GdsSharp.Lib.NonTerminals.Abstractions;
using GdsSharp.Lib.NonTerminals.Elements;
using GdsSharp.Lib.NonTerminals.Enum;
using GdsSharp.Lib.Terminals;
using GdsSharp.Lib.Terminals.Abstractions;
using GdsSharp.Lib.Terminals.Records;

namespace GdsSharp.Lib;

public class GdsParser
{
    private readonly GdsTokenStream _queue;

    public GdsParser(GdsTokenStream tokens)
    {
        _queue = tokens;
    }

    /// <summary>
    ///     Parses the GDSII token stream.
    /// </summary>
    /// <returns>Parsed file.</returns>
    /// <exception cref="ParseException">If the token stream is invalid.</exception>
    public GdsFile Parse()
    {
        var header = Get<GdsRecordHeader>().Record;
        var bgnLib = Get<GdsRecordBgnLib>().Record;
        var libName = Get<GdsRecordLibName>().Record;
        var refLibs = GetOrDefault<GdsRecordRefLibs>()?.Record;
        var fonts = GetOrDefault<GdsRecordFonts>()?.Record;
        // var attrTable = ExpectOrDefault<GdsRecordAttrTable>();
        var generations = GetOrDefault<GdsRecordGenerations>()?.Record;
        var format = GetOrDefault<GdsRecordFormat>()?.Record;
        var (units, unitsReference) = Get<GdsRecordUnits>();

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
            Structures = ParseStructures(unitsReference.Offset + unitsReference.Header.Length)
        };

        if (refLibs is not null)
            file.ReferencedLibraries = refLibs.Libraries;

        if (fonts is not null)
            file.Fonts = fonts.Fonts;

        if (generations is not null)
            file.Generations = generations.Value;

        if (format is not null)
            file.FormatType = (GdsFormatType)format.Value;

        return file;
    }

    #region Helpers

    /// <summary>
    ///     Peeks the next token and throws a <see cref="ParseException" /> if it is not of type <typeparamref name="T" />.
    /// </summary>
    /// <typeparam name="T">Expected token type.</typeparam>
    /// <returns>Peeked token of type <typeparamref name="T" />.</returns>
    /// <exception cref="ParseException">If peeked token is not of expected type.</exception>
    private (T Record, GdsTokenReference reference) Peek<T>()
        where T : IGdsRecord
    {
        var reference = _queue.Peek();
        if (reference.Record is not T record) throw new ParseException(typeof(T), reference.Record, reference.Offset);

        return (record, reference);
    }

    /// <summary>
    ///     Gets the next token and throws a <see cref="ParseException" /> if it is not of type <typeparamref name="T" />.
    /// </summary>
    /// <typeparam name="T">Expected token type.</typeparam>
    /// <returns>Next token of type <typeparamref name="T" />.</returns>
    /// <exception cref="ParseException">If next token is not of expected type.</exception>
    private (T Record, GdsTokenReference Reference) Get<T>()
        where T : IGdsRecord
    {
        var reference = _queue.Dequeue();
        if (reference.Record is not T record) throw new ParseException(typeof(T), reference.Record, reference.Offset);

        return (record, reference);
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
    private (T Record, GdsHeader Header)? GetOrDefault<T>()
        where T : IGdsRecord
    {
        if (!IsNext<T>()) return null;
        var reference = _queue.Dequeue();
        return ((T)reference.Record, reference.Header);
    }

    /// <summary>
    ///     Checks if the next token is of type <typeparamref name="T" />.
    /// </summary>
    /// <typeparam name="T">Expected token type.</typeparam>
    /// <returns>True if next token is of type <typeparamref name="T" />, false otherwise.</returns>
    private bool IsNext<T>()
        where T : IGdsRecord
    {
        return _queue.Peek().Record is T;
    }

    /// <summary>
    ///     Gets the next token of type <see cref="GdsRecordNoData" /> and throws a <see cref="ParseException" /> if it is not
    ///     of type <paramref name="type" />.
    /// </summary>
    /// <param name="type">Expected <see cref="GdsRecordNoDataType" />.</param>
    /// <exception cref="ParseException">If next token is not of expected type.</exception>
    private void GetNoData(GdsRecordNoDataType type)
    {
        var reference = Get<GdsRecordNoData>();
        if (reference.Record.Type != type) throw new ParseException(type, reference.Record.Type, reference.Reference.Offset);
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
            Attribute = Get<GdsRecordPropAttr>().Record.Value,
            Value = Get<GdsRecordPropValue>().Record.Value
        };
    }

    private GdsStrans ParseTransformation()
    {
        var strans = Get<GdsRecordSTrans>().Record;

        var returnObj = new GdsStrans
        {
            Reflection = strans.Reflection,
            AbsoluteAngle = strans.AbsoluteAngle,
            AbsoluteMagnification = strans.AbsoluteMagnification
        };

        if (GetOrDefault<GdsRecordMag>()?.Record is { } mag)
            returnObj.Magnification = mag.Value;

        if (GetOrDefault<GdsRecordAngle>()?.Record is { } angle)
            returnObj.Angle = angle.Value;

        return returnObj;
    }

    private GdsBoxElement ParseGdsBoxElement()
    {
        GetNoData(GdsRecordNoDataType.Box);
        var flags = GetOrDefault<GdsRecordElFlags>()?.Record;
        var plex = GetOrDefault<GdsRecordPlex>()?.Record;
        var layer = Get<GdsRecordLayer>().Record;
        var boxType = Get<GdsRecordBoxType>().Record;
        var xy = Get<GdsRecordXy>().Record;
        var elem = new GdsBoxElement
        {
            BoxType = boxType.Value,
            Layer = layer.Value,
            Points = xy.Coordinates.ToList()
        };

        FillElement(elem, flags, plex);

        return elem;
    }

    private GdsNodeElement ParseGdsNodeElement()
    {
        GetNoData(GdsRecordNoDataType.Node);
        var flags = GetOrDefault<GdsRecordElFlags>()?.Record;
        var plex = GetOrDefault<GdsRecordPlex>()?.Record;
        var layer = Get<GdsRecordLayer>().Record;
        var nodeType = Get<GdsRecordNodeType>().Record;
        var xy = Get<GdsRecordXy>().Record;

        var elem = new GdsNodeElement
        {
            Layer = layer.Value,
            Points = xy.Coordinates.ToList(),
            NodeType = nodeType.Value
        };

        FillElement(elem, flags, plex);

        return elem;
    }

    private GdsTextElement ParseGdsTextElement()
    {
        GetNoData(GdsRecordNoDataType.Text);
        var flags = GetOrDefault<GdsRecordElFlags>()?.Record;
        var plex = GetOrDefault<GdsRecordPlex>()?.Record;
        var layer = Get<GdsRecordLayer>().Record;
        var textType = Get<GdsRecordTextType>().Record;
        var presentation = GetOrDefault<GdsRecordPresentation>()?.Record;
        var pathType = GetOrDefault<GdsRecordPathType>()?.Record;
        var width = GetOrDefault<GdsRecordWidth>()?.Record;
        var transformation = IsNext<GdsRecordSTrans>() ? ParseTransformation() : null;
        var xy = Get<GdsRecordXy>().Record;
        var str = Get<GdsRecordString>().Record;

        var elem = new GdsTextElement
        {
            Text = str.Value,
            Layer = layer.Value,
            TextType = textType.Value,
            Points = xy.Coordinates.ToList()
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
        var flags = GetOrDefault<GdsRecordElFlags>()?.Record;
        var plex = GetOrDefault<GdsRecordPlex>()?.Record;
        var name = Get<GdsRecordSName>().Record;
        var transformation = IsNext<GdsRecordSTrans>() ? ParseTransformation() : null;
        var colRow = Get<GdsRecordColRow>().Record;
        var xy = Get<GdsRecordXy>().Record;

        var elem = new GdsArrayReferenceElement
        {
            Columns = colRow.NumCols,
            Rows = colRow.NumRows,
            Points = xy.Coordinates.ToList(), // TODO
            StructureName = name.Value
        };

        FillElement(elem, flags, plex);

        if (transformation is not null) elem.Transformation = transformation;

        return elem;
    }

    private GdsStructureReferenceElement ParseGdsStructureReferenceElement()
    {
        GetNoData(GdsRecordNoDataType.Sref);
        var flags = GetOrDefault<GdsRecordElFlags>()?.Record;
        var plex = GetOrDefault<GdsRecordPlex>()?.Record;
        var name = Get<GdsRecordSName>().Record;
        var transformation = IsNext<GdsRecordSTrans>() ? ParseTransformation() : null;
        var xy = Get<GdsRecordXy>().Record;

        var elem = new GdsStructureReferenceElement
        {
            StructureName = name.Value,
            Points = xy.Coordinates.ToList() // TDOO
        };

        FillElement(elem, flags, plex);

        if (transformation is not null) elem.Transformation = transformation;

        return elem;
    }

    private GdsPathElement ParseGdsPathElement()
    {
        GetNoData(GdsRecordNoDataType.Path);
        var flags = GetOrDefault<GdsRecordElFlags>()?.Record;
        var plex = GetOrDefault<GdsRecordPlex>()?.Record;
        var layer = Get<GdsRecordLayer>().Record;
        var dataType = Get<GdsRecordDataType>().Record;
        var pathType = GetOrDefault<GdsRecordPathType>()?.Record;
        var width = GetOrDefault<GdsRecordWidth>()?.Record;
        var xy = Get<GdsRecordXy>().Record;

        var elem = new GdsPathElement
        {
            DataType = dataType.Value,
            Layer = layer.Value,
            Points = xy.Coordinates.ToList() // TODO
        };

        FillElement(elem, flags, plex);

        if (pathType is not null) elem.PathType = (GdsPathType)pathType.Value;

        if (width is not null) elem.Width = width.Value;

        return elem;
    }

    private GdsBoundaryElement ParseGdsBoundaryElement()
    {
        GetNoData(GdsRecordNoDataType.Boundary);
        var flags = GetOrDefault<GdsRecordElFlags>()?.Record;
        var plex = GetOrDefault<GdsRecordPlex>()?.Record;
        var layer = Get<GdsRecordLayer>().Record;
        var dataType = Get<GdsRecordDataType>().Record;
        var xy = Get<GdsRecordXy>().Record;

        var elem = new GdsBoundaryElement
        {
            DataType = dataType.Value,
            Layer = layer.Value,
            Points = xy.Coordinates,
            NumPoints = xy.NumPoints
        };

        FillElement(elem, flags, plex);

        return elem;
    }

    private GdsElement ParseGdsElement()
    {
        var (token, reference) = Peek<GdsRecordNoData>();

        IGdsElement elem = token.Type switch
        {
            GdsRecordNoDataType.Box => ParseGdsBoxElement(),
            GdsRecordNoDataType.Node => ParseGdsNodeElement(),
            GdsRecordNoDataType.Text => ParseGdsTextElement(),
            GdsRecordNoDataType.Aref => ParseGdsArrayReferenceElement(),
            GdsRecordNoDataType.Sref => ParseGdsStructureReferenceElement(),
            GdsRecordNoDataType.Path => ParseGdsPathElement(),
            GdsRecordNoDataType.Boundary => ParseGdsBoundaryElement(),
            _ => throw new ParseException(token.Type, reference.Offset)
        };

        var properties = new List<GdsProperty>();
        while (_queue.Peek().Record is not GdsRecordNoData { Type: GdsRecordNoDataType.EndEl }) properties.Add(ParseGdsProperty());

        GetNoData(GdsRecordNoDataType.EndEl);

        return new GdsElement
        {
            Element = elem,
            Properties = properties
        };
    }

    private IEnumerable<GdsElement> ParseElements(long offset)
    {
        var oldPosition = _queue.SetPosition(offset);
        try
        {
            while (_queue.Peek().Record is not GdsRecordNoData { Type: GdsRecordNoDataType.EndStr })
                yield return ParseGdsElement();
            GetNoData(GdsRecordNoDataType.EndStr);
        }
        finally
        {
            _queue.SetPosition(oldPosition);
        }
    }

    private GdsStructure ParseGdsStructure()
    {
        var bgnStr = Get<GdsRecordBgnStr>().Record;
        var (name, reference) = Get<GdsRecordStrName>();

        var elements = ParseElements(reference.Offset + reference.Header.Length);
        
        // Skip elements until the end of the structure
        while (_queue.Dequeue().Record is not GdsRecordNoData { Type: GdsRecordNoDataType.EndStr })
        {
        }

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

    private IEnumerable<GdsStructure> ParseStructures(long position)
    {
        var oldPosition = _queue.SetPosition(position);
        try
        {
            while (_queue.Peek().Record is not GdsRecordNoData { Type: GdsRecordNoDataType.EndLib })
                yield return ParseGdsStructure();
            GetNoData(GdsRecordNoDataType.EndLib);
        }
        finally
        {
            _queue.SetPosition(oldPosition);
        }
    }

    #endregion
}