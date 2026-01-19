using System.Buffers;
using GdsSharp.Lib.Reading.Enum;
using GdsSharp.Lib.Reading.Models;
using GdsSharp.Lib.Reading.TokenStream;

namespace GdsSharp.Lib.Reading;

public sealed class GdsParser(GdsTokenStream tokenStream) : IDisposable
{
    public void Dispose()
    {
        tokenStream.Dispose();
    }

    public void Parse(IParserConsumer consumer)
    {
        Expect(GdsRecordTypes.Header);
        var version = tokenStream.ReadInt16();

        Expect(GdsRecordTypes.BeginLibrary);
        var (lastMod, lastAcc) = ReadBgnTimestampPair();

        var libNameHdr = Expect(GdsRecordTypes.LibraryName);
        var libName = tokenStream.ReadString(libNameHdr);

        string[]? refLibs = null;
        string[]? fonts = null;
        short? generations = null;
        int? format = null;

        while (tokenStream.TryPeek(out var peek) && peek.Code is not GdsRecordTypes.Units)
        {
            var h = tokenStream.Read();

            switch (h.Code)
            {
                case GdsRecordTypes.ReferencedLibraries:
                    refLibs = ReadStringArray(h);
                    break;

                case GdsRecordTypes.Fonts:
                    fonts = ReadStringArray(h);
                    break;

                case GdsRecordTypes.Generations:
                    generations = tokenStream.ReadInt16();
                    break;

                case GdsRecordTypes.Format:
                    format = tokenStream.ReadInt16();
                    break;

                default:
                    throw new InvalidDataException($"Unexpected record code 0x{h.Code:X} at 0x{h.Offset:X}.");
            }
        }

        Expect(GdsRecordTypes.Units);
        var (userUnits, physicalUnits) = ReadUnits();

        var libraryInfo = new GdsLibraryInfo(
            version,
            libName,
            lastMod,
            lastAcc,
            refLibs ?? [],
            fonts ?? [],
            null,
            generations,
            userUnits,
            physicalUnits,
            format.HasValue ? (GdsFormatType)format.Value : null
        );
        consumer.OnBeginLibrary(in libraryInfo);

        while (true)
        {
            var next = tokenStream.Peek();

            if (next.Code == GdsRecordTypes.EndLibrary)
            {
                tokenStream.Read();
                consumer.OnEndLibrary();
                return;
            }

            ParseStructure(consumer);
        }
    }

    private void ParseStructure(IParserConsumer consumer)
    {
        Expect(GdsRecordTypes.BeginStruct);
        var (creation, modification) = ReadBgnTimestampPair();

        var nameHeader = Expect(GdsRecordTypes.StructName);
        var name = tokenStream.ReadString(nameHeader);

        var structureInfo = new GdsStructureInfo
        {
            Name = name,
            CreationTime = creation,
            ModificationTime = modification
        };
        consumer.OnBeginStructure(in structureInfo);

        while (true)
        {
            var header = tokenStream.Peek();

            if (header.Code == GdsRecordTypes.EndStruct)
            {
                tokenStream.Read();
                consumer.OnEndStructure();
                return;
            }

            ParseStructureElement(consumer);
        }
    }

    private void ParseStructureElement(IParserConsumer consumer)
    {
        var first = tokenStream.Read();

        switch (first.Code)
        {
            case GdsRecordTypes.Boundary:
                ParseBoundary(consumer);
                break;

            case GdsRecordTypes.Path:
                ParsePath(consumer);
                break;

            case GdsRecordTypes.StructureReference:
                ParseSref(consumer);
                break;

            case GdsRecordTypes.ArrayReference:
                ParseAref(consumer);
                break;

            case GdsRecordTypes.Text:
                ParseText(consumer);
                break;

            case GdsRecordTypes.Box:
                ParseBox(consumer);
                break;

            case GdsRecordTypes.Node:
                ParseNode(consumer);
                break;

            default:
                throw new InvalidDataException($"Unexpected element start code 0x{first.Code:X} at 0x{first.Offset:X}.");
        }
    }

    private GdsElementCommon? ReadGdsElementCommonOptionals()
    {
        bool? external = null;
        bool? template = null;
        int? plex = null;

        while (tokenStream.TryPeek(out var p))
        {
            if (p.Code == GdsRecordTypes.ElementFlags)
            {
                tokenStream.Read();
                var flags = tokenStream.ReadUInt16();
                external = (flags & 0b10) != 0;
                template = (flags & 0b1) != 0;
                continue;
            }

            if (p.Code == GdsRecordTypes.Plex)
            {
                tokenStream.Read();
                plex = tokenStream.ReadInt32();
                continue;
            }

            break;
        }

        if (!external.HasValue && !template.HasValue && !plex.HasValue)
            return null;

        return new GdsElementCommon(external, template, plex);
    }

    private void ParseBoundary(IParserConsumer consumer)
    {
        var common = ReadGdsElementCommonOptionals();

        Expect(GdsRecordTypes.Layer);
        var layer = tokenStream.ReadInt16();

        Expect(GdsRecordTypes.DataType);
        var dataType = tokenStream.ReadInt16();

        var xyHeader = Expect(GdsRecordTypes.Xy);
        var numPoints = xyHeader.PayloadLength / 8;

        if (numPoints <= 256)
        {
            Span<GdsPoint> points = stackalloc GdsPoint[numPoints];
            tokenStream.ReadXy(xyHeader, points);

            if (points[0] != points[numPoints - 1])
                throw new InvalidDataException($"Boundary's first and last point must coincide, found first point {points[0]} and last point {points[numPoints - 1]} at 0x{xyHeader.Offset:X}.");

            consumer.OnBeginElement(GdsElementKind.Boundary, common);
            consumer.OnBoundary(layer, dataType, points);
            ConsumePropertiesAndEndElement(consumer);
            return;
        }

        var rentedPoints = ArrayPool<GdsPoint>.Shared.Rent(numPoints);
        tokenStream.ReadXy(xyHeader, rentedPoints.AsSpan(0, numPoints));

        if (rentedPoints[0] != rentedPoints[numPoints - 1])
            throw new InvalidDataException(
                $"Boundary's first and last point must coincide, found first point {rentedPoints[0]} and last point {rentedPoints[numPoints - 1]} at 0x{xyHeader.Offset:X}.");

        consumer.OnBeginElement(GdsElementKind.Boundary, common);
        consumer.OnBoundary(layer, dataType, rentedPoints.AsSpan(0, numPoints));
        ConsumePropertiesAndEndElement(consumer);
        ArrayPool<GdsPoint>.Shared.Return(rentedPoints);
    }

    private void ParsePath(IParserConsumer consumer)
    {
        var common = ReadGdsElementCommonOptionals();

        Expect(GdsRecordTypes.Layer);
        var layer = tokenStream.ReadInt16();

        Expect(GdsRecordTypes.DataType);
        var dataType = tokenStream.ReadInt16();

        short? pathTypeShort = null;
        int? width = null;
        while (tokenStream.TryPeek(out var p))
        {
            if (p.Code == GdsRecordTypes.PathType)
            {
                tokenStream.Read();
                pathTypeShort = tokenStream.ReadInt16();
                continue;
            }

            if (p.Code == GdsRecordTypes.Width)
            {
                tokenStream.Read();
                width = tokenStream.ReadInt32();
                continue;
            }

            break;
        }

        var pathType = pathTypeShort.HasValue ? (GdsPathType?)pathTypeShort.Value : null;

        var xyHeader = Expect(GdsRecordTypes.Xy);
        var numPoints = xyHeader.PayloadLength / 8;

        if (numPoints <= 256)
        {
            Span<GdsPoint> points = stackalloc GdsPoint[numPoints];
            tokenStream.ReadXy(xyHeader, points);
            consumer.OnBeginElement(GdsElementKind.Path, common);
            consumer.OnPath(layer, dataType, pathType, width, points);
            ConsumePropertiesAndEndElement(consumer);
            return;
        }

        var rentedPoints = ArrayPool<GdsPoint>.Shared.Rent(numPoints);
        try
        {
            tokenStream.ReadXy(xyHeader, rentedPoints.AsSpan(0, numPoints));
            consumer.OnBeginElement(GdsElementKind.Path, common);
            consumer.OnPath(layer, dataType, pathType, width, rentedPoints.AsSpan(0, numPoints));
            ConsumePropertiesAndEndElement(consumer);
        }
        finally
        {
            ArrayPool<GdsPoint>.Shared.Return(rentedPoints);
        }
    }

    private void ParseSref(IParserConsumer consumer)
    {
        var common = ReadGdsElementCommonOptionals();

        var nameHeader = Expect(GdsRecordTypes.StructureName);
        var structureName = tokenStream.ReadString(nameHeader);

        var strans = TryReadStrans();

        var xyHeader = Expect(GdsRecordTypes.Xy);
        var numPoints = xyHeader.PayloadLength / 8;
        if (numPoints != GdsGlobals.StructureReferencePointCount)
            throw new InvalidDataException($"Structure reference must have exactly {GdsGlobals.TextPointCount} coordinate point, found {numPoints} at 0x{xyHeader.Offset:X}.");

        Span<GdsPoint> pts = stackalloc GdsPoint[numPoints];
        tokenStream.ReadXy(xyHeader, pts);
        consumer.OnBeginElement(GdsElementKind.StructureReference, common);
        consumer.OnSref(structureName, strans, pts);
        ConsumePropertiesAndEndElement(consumer);
    }

    private void ParseAref(IParserConsumer consumer)
    {
        var common = ReadGdsElementCommonOptionals();

        var nameHeader = Expect(GdsRecordTypes.StructureName);
        var targetName = tokenStream.ReadString(nameHeader);

        var strans = TryReadStrans();

        Expect(GdsRecordTypes.ColumnRow);
        var (columns, rows) = ReadColRow();

        var xyHeader = Expect(GdsRecordTypes.Xy);
        var numPoints = xyHeader.PayloadLength / 8;
        if (numPoints != GdsGlobals.ArrayReferencePointCount)
            throw new InvalidDataException($"Array reference must have exactly {GdsGlobals.ArrayReferencePointCount} coordinate points, found {numPoints} at 0x{xyHeader.Offset:X}.");

        Span<GdsPoint> pts = stackalloc GdsPoint[numPoints];
        tokenStream.ReadXy(xyHeader, pts);
        consumer.OnBeginElement(GdsElementKind.ArrayReference, common);
        consumer.OnAref(targetName, strans, columns, rows, pts);
        ConsumePropertiesAndEndElement(consumer);
    }

    private void ParseText(IParserConsumer consumer)
    {
        var common = ReadGdsElementCommonOptionals();

        Expect(GdsRecordTypes.Layer);
        var layer = tokenStream.ReadInt16();

        Expect(GdsRecordTypes.TextType);
        var textType = tokenStream.ReadInt16();

        PresentationInfo? pres = null;
        short? pathTypeShort = null;
        int? width = null;

        while (tokenStream.TryPeek(out var p))
        {
            if (p.Code == GdsRecordTypes.Presentation)
            {
                tokenStream.Read();
                pres = ReadPresentation();
                continue;
            }

            if (p.Code == GdsRecordTypes.PathType)
            {
                tokenStream.Read();
                pathTypeShort = tokenStream.ReadInt16();
                continue;
            }

            if (p.Code == GdsRecordTypes.Width)
            {
                tokenStream.Read();
                width = tokenStream.ReadInt32();
                continue;
            }

            break;
        }

        var pathType = pathTypeShort.HasValue ? (GdsPathType?)pathTypeShort.Value : null;

        var strans = TryReadStrans();

        var xyHeader = Expect(GdsRecordTypes.Xy);
        var numPoints = xyHeader.PayloadLength / 8;
        if (numPoints != GdsGlobals.TextPointCount)
            throw new InvalidDataException($"Text element must have exactly {GdsGlobals.TextPointCount} coordinate point, found {numPoints} at 0x{xyHeader.Offset:X}.");

        Span<GdsPoint> points = stackalloc GdsPoint[numPoints];
        tokenStream.ReadXy(xyHeader, points);

        var strHdr = Expect(GdsRecordTypes.String);
        var text = tokenStream.ReadString(strHdr);

        consumer.OnBeginElement(GdsElementKind.Text, common);
        consumer.OnText(layer, textType, pres, pathType, width, strans, points, text);
        ConsumePropertiesAndEndElement(consumer);
    }

    private void ParseBox(IParserConsumer consumer)
    {
        var common = ReadGdsElementCommonOptionals();

        Expect(GdsRecordTypes.Layer);
        var layer = tokenStream.ReadInt16();

        Expect(GdsRecordTypes.BoxType);
        var boxType = tokenStream.ReadInt16();

        var xyHeader = Expect(GdsRecordTypes.Xy);
        var numPoints = xyHeader.PayloadLength / 8;
        if (numPoints != GdsGlobals.BoxPointCount)
            throw new InvalidDataException($"Box element must have exactly {GdsGlobals.BoxPointCount} coordinate points, found {numPoints} at 0x{xyHeader.Offset:X}.");

        Span<GdsPoint> points = stackalloc GdsPoint[numPoints];
        tokenStream.ReadXy(xyHeader, points);
        consumer.OnBeginElement(GdsElementKind.Box, common);
        consumer.OnBox(layer, boxType, points);
        ConsumePropertiesAndEndElement(consumer);
    }

    private void ParseNode(IParserConsumer consumer)
    {
        var common = ReadGdsElementCommonOptionals();

        Expect(GdsRecordTypes.Layer);
        var layer = tokenStream.ReadInt16();

        Expect(GdsRecordTypes.NodeType);
        var nodeType = tokenStream.ReadInt16();

        var xyHeader = Expect(GdsRecordTypes.Xy);
        var numPoints = xyHeader.PayloadLength / 8;

        if (numPoints <= 256)
        {
            Span<GdsPoint> points = stackalloc GdsPoint[numPoints];
            tokenStream.ReadXy(xyHeader, points);
            consumer.OnBeginElement(GdsElementKind.Node, common);
            consumer.OnNode(layer, nodeType, points);
            ConsumePropertiesAndEndElement(consumer);
            return;
        }

        var rentedPoints = ArrayPool<GdsPoint>.Shared.Rent(numPoints);
        tokenStream.ReadXy(xyHeader, rentedPoints.AsSpan(0, numPoints));
        consumer.OnBeginElement(GdsElementKind.Node, common);
        consumer.OnNode(layer, nodeType, rentedPoints.AsSpan(0, numPoints));
        ConsumePropertiesAndEndElement(consumer);
        ArrayPool<GdsPoint>.Shared.Return(rentedPoints);
    }

    private void ConsumePropertiesAndEndElement(IParserConsumer consumer)
    {
        while (true)
        {
            var header = tokenStream.Peek();
            if (header.Code == GdsRecordTypes.EndElement)
            {
                tokenStream.Read();
                consumer.OnEndElement();
                return;
            }

            Expect(GdsRecordTypes.PropertyAttribute);
            var attribute = tokenStream.ReadInt16();

            var valueHeader = Expect(GdsRecordTypes.PropertyValue);
            var value = tokenStream.ReadString(valueHeader);

            consumer.OnProperty(attribute, value);
        }
    }

    private GdsStransInfo? TryReadStrans()
    {
        if (!tokenStream.TryPeek(out var p) || p.Code != GdsRecordTypes.Strans)
            return null;

        tokenStream.Read();
        var flags = tokenStream.ReadUInt16();

        var s = new GdsStransInfo(
            (flags & 0b10000000_00000000) != 0,
            (flags & 0b100) != 0,
            (flags & 0b10) != 0
        );

        if (tokenStream.TryPeek(out var magPeek) && magPeek.Code == GdsRecordTypes.Magnification)
        {
            tokenStream.Read();
            s = s with { Magnification = tokenStream.ReadDouble() };
        }

        if (tokenStream.TryPeek(out var angPeek) && angPeek.Code == GdsRecordTypes.Angle)
        {
            tokenStream.Read();
            s = s with { Angle = tokenStream.ReadDouble() };
        }

        return s;
    }

    private GdsTokenHeader Expect(ushort code)
    {
        var header = tokenStream.Read();
        if (header.Code != code)
            throw new InvalidDataException($"Expected code 0x{code:X}, got 0x{header.Code:X} at 0x{header.Offset:X}.");
        return header;
    }

    private (DateTime A, DateTime B) ReadBgnTimestampPair()
    {
        var creationYear = tokenStream.ReadInt16();
        var creationMonth = tokenStream.ReadInt16();
        var creationDay = tokenStream.ReadInt16();
        var creationHour = tokenStream.ReadInt16();
        var creationMinute = tokenStream.ReadInt16();
        var creationSecond = tokenStream.ReadInt16();

        var modificationYear = tokenStream.ReadInt16();
        var modificationMonth = tokenStream.ReadInt16();
        var modificationDay = tokenStream.ReadInt16();
        var modificationHour = tokenStream.ReadInt16();
        var modificationMinute = tokenStream.ReadInt16();
        var modificationSecond = tokenStream.ReadInt16();

        var creation = new DateTime(creationYear, creationMonth, creationDay, creationHour, creationMinute, creationSecond, DateTimeKind.Utc);
        var modification = new DateTime(modificationYear, modificationMonth, modificationDay, modificationHour, modificationMinute, modificationSecond, DateTimeKind.Utc);
        return (creation, modification);
    }

    private (double UserUnits, double PhysicalUnits) ReadUnits()
    {
        var userUnits = tokenStream.ReadDouble();
        var physicalUnits = tokenStream.ReadDouble();
        return (userUnits, physicalUnits);
    }

    private string[] ReadStringArray(GdsTokenHeader h)
    {
        var numStrings = h.PayloadLength / 44;
        var result = new string[numStrings];
        for (var i = 0; i < numStrings; i++) result[i] = tokenStream.ReadString(44).TrimEnd('\0');

        return result;
    }

    private (short Cols, short Rows) ReadColRow()
    {
        var cols = tokenStream.ReadInt16();
        var rows = tokenStream.ReadInt16();
        return (cols, rows);
    }

    private PresentationInfo ReadPresentation()
    {
        var packed = tokenStream.ReadUInt16();
        var fontNumber = (packed & 0b110000) >> 4;
        var verticalPresentation = (packed & 0b1100) >> 2;
        var horizontalPresentation = packed & 0b11;

        return new PresentationInfo(
            (short)fontNumber,
            (short)horizontalPresentation,
            (short)verticalPresentation
        );
    }
}