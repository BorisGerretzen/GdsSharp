using GdsSharp.Lib.Abstractions;
using GdsSharp.Lib.NonTerminals;
using GdsSharp.Lib.NonTerminals.Elements;
using GdsSharp.Lib.NonTerminals.Enum;
using GdsSharp.Lib.Terminals.Abstractions;
using GdsSharp.Lib.Terminals.Records;

namespace GdsSharp.Lib;

public class GdsTokenWriter
{
    private readonly GdsFile _file;

    public GdsTokenWriter(GdsFile file)
    {
        _file = file;
    }

    public IEnumerable<IGdsRecord> Tokenize()
    {
        yield return new GdsRecordHeader { Value = _file.Version };
        yield return new GdsRecordBgnLib
        {
            LastModificationTimeYear = (short)_file.LastModificationTime.Year,
            LastModificationTimeMonth = (short)_file.LastModificationTime.Month,
            LastModificationTimeDay = (short)_file.LastModificationTime.Day,
            LastModificationTimeHour = (short)_file.LastModificationTime.Hour,
            LastModificationTimeMinute = (short)_file.LastModificationTime.Minute,
            LastModificationTimeSecond = (short)_file.LastModificationTime.Second,
            LastAccessTimeYear = (short)_file.LastAccessTime.Year,
            LastAccessTimeMonth = (short)_file.LastAccessTime.Month,
            LastAccessTimeDay = (short)_file.LastAccessTime.Day,
            LastAccessTimeHour = (short)_file.LastAccessTime.Hour,
            LastAccessTimeMinute = (short)_file.LastAccessTime.Minute,
            LastAccessTimeSecond = (short)_file.LastAccessTime.Second
        };

        yield return new GdsRecordLibName { Value = _file.LibraryName };

        if (_file.ReferencedLibraries.Count > 0) yield return new GdsRecordRefLibs { Libraries = _file.ReferencedLibraries };

        if (_file.Fonts.Count > 0) yield return new GdsRecordFonts { Fonts = _file.Fonts };

        // attrtable

        if (_file.Generations != 3) yield return new GdsRecordGenerations { Value = _file.Generations };

        if (_file.FormatType != GdsFormatType.GdsArchive) yield return new GdsRecordFormat { Value = (short)_file.FormatType };

        yield return new GdsRecordUnits { PhysicalUnits = _file.PhysicalUnits, UserUnits = _file.UserUnits };

        foreach (var records in _file.Structures.SelectMany(TokenizeStructure)) yield return records;

        yield return new GdsRecordNoData { Type = GdsRecordNoDataType.EndLib };
    }

    private IEnumerable<IGdsRecord> TokenizeProperty(GdsProperty property)
    {
        yield return new GdsRecordPropAttr
        {
            Value = property.Attribute
        };

        yield return new GdsRecordPropValue
        {
            Value = property.Value
        };
    }

    private IEnumerable<IGdsRecord> TokenizeTransform(GdsStrans transform)
    {
        if (transform is { Reflection: false, AbsoluteMagnification: false, AbsoluteAngle: false } &&
            Math.Abs(transform.Magnification - 1.0d) < 1e-9 && Math.Abs(transform.Angle) < 1e-9)
            yield break;

        yield return new GdsRecordSTrans
        {
            Reflection = transform.Reflection,
            AbsoluteMagnification = transform.AbsoluteMagnification,
            AbsoluteAngle = transform.AbsoluteAngle
        };

        if (transform.Magnification - 1.0d > 1e-9)
            yield return new GdsRecordMag
            {
                Value = transform.Magnification
            };

        if (Math.Abs(transform.Angle) > 1e-9)
            yield return new GdsRecordAngle
            {
                Value = transform.Angle
            };
    }

    private IEnumerable<IGdsRecord> TokenizeTextElement(GdsTextElement textElement)
    {
        yield return new GdsRecordNoData { Type = GdsRecordNoDataType.Text };

        foreach (var record in TokenizeLayeredElementBase(textElement)) yield return record;

        yield return new GdsRecordTextType { Value = textElement.TextType };

        if (textElement.Font != GdsFont.Font0 ||
            textElement.VerticalJustification != GdsVerticalJustification.Top ||
            textElement.HorizontalJustification != GdsHorizontalJustification.Left)
            yield return new GdsRecordPresentation
            {
                HorizontalPresentation = (int)textElement.HorizontalJustification,
                VerticalPresentation = (int)textElement.VerticalJustification,
                FontNumber = (int)textElement.Font
            };

        if (textElement.PathType != GdsPathType.Square)
            yield return new GdsRecordPathType
            {
                Value = (short)textElement.PathType
            };

        if (textElement.Width != 0)
            yield return new GdsRecordWidth
            {
                Value = textElement.Width
            };

        foreach (var record in TokenizeTransform(textElement.Transformation)) yield return record;

        yield return new GdsRecordXy { Coordinates = textElement.Points.AsTuplePoints() };
        yield return new GdsRecordString { Value = textElement.Text };
    }

    private IEnumerable<IGdsRecord> TokenizeBoxElement(GdsBoxElement boxElement)
    {
        yield return new GdsRecordNoData { Type = GdsRecordNoDataType.Box };

        foreach (var record in TokenizeLayeredElementBase(boxElement)) yield return record;

        yield return new GdsRecordBoxType { Value = boxElement.BoxType };

        yield return new GdsRecordXy { Coordinates = boxElement.Points.AsTuplePoints() };
    }

    private IEnumerable<IGdsRecord> TokenizeNodeElement(GdsNodeElement nodeElement)
    {
        yield return new GdsRecordNoData { Type = GdsRecordNoDataType.Node };

        foreach (var record in TokenizeLayeredElementBase(nodeElement)) yield return record;

        yield return new GdsRecordNodeType { Value = nodeElement.NodeType };

        yield return new GdsRecordXy { Coordinates = nodeElement.Points.AsTuplePoints() };
    }

    private IEnumerable<IGdsRecord> TokenizeArrayReferenceElement(GdsArrayReferenceElement arrayReferenceElement)
    {
        yield return new GdsRecordNoData { Type = GdsRecordNoDataType.Aref };

        foreach (var record in TokenizeElementBase(arrayReferenceElement)) yield return record;

        yield return new GdsRecordSName { Value = arrayReferenceElement.StructureName };

        foreach (var record in TokenizeTransform(arrayReferenceElement.Transformation)) yield return record;

        yield return new GdsRecordColRow { NumCols = (short)arrayReferenceElement.Columns, NumRows = (short)arrayReferenceElement.Rows };

        yield return new GdsRecordXy { Coordinates = arrayReferenceElement.Points.AsTuplePoints() };
    }

    private IEnumerable<IGdsRecord> TokenizeStructureReferenceElement(GdsStructureReferenceElement element)
    {
        yield return new GdsRecordNoData { Type = GdsRecordNoDataType.Sref };

        foreach (var record in TokenizeElementBase(element)) yield return record;

        yield return new GdsRecordSName { Value = element.StructureName };

        foreach (var record in TokenizeTransform(element.Transformation)) yield return record;

        yield return new GdsRecordXy { Coordinates = element.Points.AsTuplePoints() };
    }

    private IEnumerable<IGdsRecord> TokenizePathElement(GdsPathElement element)
    {
        yield return new GdsRecordNoData { Type = GdsRecordNoDataType.Path };

        foreach (var record in TokenizeLayeredElementBase(element)) yield return record;

        yield return new GdsRecordDataType { Value = element.DataType };

        if (element.PathType != GdsPathType.Square) yield return new GdsRecordPathType { Value = (short)element.PathType };

        if (element.Width != 0) yield return new GdsRecordWidth { Value = element.Width };

        yield return new GdsRecordXy { Coordinates = element.Points.AsTuplePoints() };
    }

    private IEnumerable<IGdsRecord> TokenizeBoundaryElement(GdsBoundaryElement element)
    {
        yield return new GdsRecordNoData { Type = GdsRecordNoDataType.Boundary };

        foreach (var record in TokenizeLayeredElementBase(element)) yield return record;

        yield return new GdsRecordDataType { Value = element.DataType };

        yield return new GdsRecordXy { Coordinates = element.Points.AsTuplePoints() };
    }

    private IEnumerable<IGdsRecord> TokenizeElement(GdsElement element)
    {
        var records = element.Element switch
        {
            GdsTextElement textElement => TokenizeTextElement(textElement),
            GdsBoxElement boxElement => TokenizeBoxElement(boxElement),
            GdsNodeElement nodeElement => TokenizeNodeElement(nodeElement),
            GdsArrayReferenceElement arrayReferenceElement => TokenizeArrayReferenceElement(arrayReferenceElement),
            GdsStructureReferenceElement structureReferenceElement => TokenizeStructureReferenceElement(structureReferenceElement),
            GdsPathElement pathElement => TokenizePathElement(pathElement),
            GdsBoundaryElement boundaryElement => TokenizeBoundaryElement(boundaryElement),
            _ => throw new ArgumentOutOfRangeException(nameof(element), $"Cannot tokenize element of type '{element.GetType()}'")
        };

        foreach (var record in records) yield return record;

        foreach (var record in element.Properties.SelectMany(TokenizeProperty)) yield return record;

        yield return new GdsRecordNoData { Type = GdsRecordNoDataType.EndEl };
    }

    private IEnumerable<IGdsRecord> TokenizeStructure(GdsStructure structure)
    {
        yield return new GdsRecordBgnStr
        {
            CreationTimeYear = (short)structure.CreationTime.Year,
            CreationTimeMonth = (short)structure.CreationTime.Month,
            CreationTimeDay = (short)structure.CreationTime.Day,
            CreationTimeHour = (short)structure.CreationTime.Hour,
            CreationTimeMinute = (short)structure.CreationTime.Minute,
            CreationTimeSecond = (short)structure.CreationTime.Second,
            LastModificationTimeYear = (short)structure.ModificationTime.Year,
            LastModificationTimeMonth = (short)structure.ModificationTime.Month,
            LastModificationTimeDay = (short)structure.ModificationTime.Day,
            LastModificationTimeHour = (short)structure.ModificationTime.Hour,
            LastModificationTimeMinute = (short)structure.ModificationTime.Minute,
            LastModificationTimeSecond = (short)structure.ModificationTime.Second
        };

        yield return new GdsRecordStrName { Value = structure.Name };

        foreach (var record in structure.Elements.SelectMany(TokenizeElement)) yield return record;

        yield return new GdsRecordNoData { Type = GdsRecordNoDataType.EndStr };
    }

    #region Helpers

    private IEnumerable<IGdsRecord> TokenizeElementBase(IGdsElement element)
    {
        if (element.ExternalData || element.TemplateData)
            yield return new GdsRecordElFlags
            {
                ExternalData = element.ExternalData,
                TemplateData = element.TemplateData
            };

        if (element.PlexNumber != 0)
            yield return new GdsRecordPlex
            {
                Value = element.PlexNumber
            };
    }

    private IEnumerable<IGdsRecord> TokenizeLayeredElementBase(IGdsLayeredElement element)
    {
        foreach (var record in TokenizeElementBase(element)) yield return record;

        yield return new GdsRecordLayer
        {
            Value = element.Layer
        };
    }

    #endregion
}