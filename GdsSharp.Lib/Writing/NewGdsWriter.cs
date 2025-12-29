using System.Buffers;
using System.Buffers.Binary;
using System.Runtime.InteropServices;
using System.Text;
using GdsSharp.Lib.Binary;
using GdsSharp.Lib.InternalDb;
using GdsSharp.Lib.InternalDb.Builder;
using GdsSharp.Lib.InternalDb.VertexStore;
using GdsSharp.Lib.Obsolete.NonTerminals.Enum;
using GdsSharp.Lib.Reading.Models;
using GdsSharp.Lib.Reading.TokenStream;

namespace GdsSharp.Lib.Writing;

public sealed class NewGdsWriter
{
    private readonly GdsWriteBuffer _buffer;

    private readonly Stream _output;
    private readonly BufferedGdsBinaryWriter _writer;

    public NewGdsWriter(Stream output)
    {
        _output = output;
        _buffer = new GdsWriteBuffer();
        _writer = new BufferedGdsBinaryWriter(_buffer);
    }

    /// <summary>
    /// Writes the given GDSII library to the underlying stream.
    /// </summary>
    /// <param name="library">Library to write.</param>
    /// <param name="vertexReader">Reader for the vertex store of the library.</param>
    /// <exception cref="ArgumentNullException">If vertexReader or library are null.</exception>
    public void Write(GdsLibrary library, IGdsVertexStoreReader vertexReader)
    {
        if (library is null) throw new ArgumentNullException(nameof(library));
        if (vertexReader is null) throw new ArgumentNullException(nameof(vertexReader));

        WriteRecord(GdsRecordTypes.Header, w => { w.Write(library.Info.Version); });
        WriteRecord(GdsRecordTypes.BeginLibrary, w => { WriteTimestampPair(w, library.Info.ModificationTime, library.Info.AccessTime); });
        WriteRecord(GdsRecordTypes.LibraryName, w => { WriteGdsString(w, library.Info.Name); });

        if (library.Info.ReferencedLibraries is { Count: > 0 } refLibs)
        {
            WriteRecord(GdsRecordTypes.ReferencedLibraries, w =>
            {
                foreach (var s in refLibs)
                    WriteFixedString(w, s, 44);
            });
        }

        if (library.Info.Fonts is { Count: > 0 } fonts)
        {
            WriteRecord(GdsRecordTypes.Fonts, w =>
            {
                foreach (var s in fonts)
                    WriteFixedString(w, s, 44);
            });
        }

        if (library.Info.Generations.HasValue)
        {
            WriteRecord(GdsRecordTypes.Generations, w => { w.Write(library.Info.Generations.Value); });
        }

        WriteRecord(GdsRecordTypes.Format, w => { w.Write((short)library.Info.FormatType); });
        WriteRecord(GdsRecordTypes.Units, w =>
        {
            w.Write(library.Info.UserUnits);
            w.Write(library.Info.PhysicalUnits);
        });

        var propertiesByElement = library.PropertiesByElementIndex.Value;
        foreach (var s in library.Structures)
        {
            WriteStructure(library, s, vertexReader, propertiesByElement);
        }

        WriteRecord(GdsRecordTypes.EndLibrary, payloadWriter: null);
    }

    private void WriteStructure(GdsLibrary library, GdsStructure structure, IGdsVertexStoreReader vertexReader, Dictionary<int, PropertyRecord[]> propertiesByElement)
    {
        var info = structure.Info;

        WriteRecord(GdsRecordTypes.BeginStruct, w => { WriteTimestampPair(w, info.CreationTime, info.ModificationTime); });
        WriteRecord(GdsRecordTypes.StructName, w => { WriteGdsString(w, info.Name); });

        var elements = library.Elements.AsSpan().Slice(structure.ElementStartIndex, structure.ElementCount);
        foreach (var element in elements)
        {
            switch (element.Kind)
            {
                case ElementKind.ARef:
                {
                    var aref = library.ArrayReferences[element.Index];
                    WriteRecord(GdsRecordTypes.ArrayReference, null);
                    WriteCommon(element.Common);
                    WriteRecord(GdsRecordTypes.StructureName, w => WriteGdsString(w, aref.TargetName));
                    WriteStrans(aref.Strans);
                    WriteRecord(GdsRecordTypes.ColumnRow, w =>
                    {
                        w.Write(aref.Columns);
                        w.Write(aref.Rows);
                    });
                    WriteRecord(GdsRecordTypes.Xy, w =>
                    {
                        WritePoint(w, aref.Origin);
                        WritePoint(w, aref.ColumnVector);
                        WritePoint(w, aref.RowVector);
                    });
                    break;
                }
                case ElementKind.Boundary:
                {
                    var boundary = library.Boundaries[element.Index];
                    WriteRecord(GdsRecordTypes.Boundary, null);
                    WriteCommon(element.Common);
                    WriteLayer(boundary.Layer);
                    WriteDataType(boundary.DataType);
                    WriteXyFromStore(boundary.VertexOffset, boundary.VertexCount, vertexReader);
                    break;
                }
                case ElementKind.Box:
                {
                    var box = library.Boxes[element.Index];
                    WriteRecord(GdsRecordTypes.Box, null);
                    WriteCommon(element.Common);
                    WriteLayer(box.Layer);
                    WriteRecord(GdsRecordTypes.BoxType, w => w.Write(box.BoxType));
                    WriteXyFromStore(box.VertexOffset, box.VertexCount, vertexReader);
                    break;
                }
                case ElementKind.Node:
                {
                    var node = library.Nodes[element.Index];
                    WriteRecord(GdsRecordTypes.Node, null);
                    WriteCommon(element.Common);
                    WriteLayer(node.Layer);
                    WriteRecord(GdsRecordTypes.NodeType, w => w.Write(node.NodeType));
                    WriteXyFromStore(node.VertexOffset, node.VertexCount, vertexReader);
                    break;
                }
                case ElementKind.Path:
                {
                    var path = library.Paths[element.Index];
                    WriteRecord(GdsRecordTypes.Path, null);
                    WriteCommon(element.Common);
                    WriteLayer(path.Layer);
                    WriteDataType(path.DataType);
                    if (path.PathType is { } pt)
                        WritePathType(pt);
                    if (path.Width is { } w)
                        WriteWidth(w);
                    WriteXyFromStore(path.VertexOffset, path.VertexCount, vertexReader);
                    break;
                }
                case ElementKind.SRef:
                {
                    var sref = library.StructureReferences[element.Index];
                    WriteRecord(GdsRecordTypes.StructureReference, null);
                    WriteCommon(element.Common);
                    WriteRecord(GdsRecordTypes.StructureName, w => WriteGdsString(w, sref.TargetName));
                    WriteStrans(sref.Strans);
                    WriteRecord(GdsRecordTypes.Xy, w => { WritePoint(w, sref.Origin); });
                    break;
                }
                case ElementKind.Text:
                {
                    var textRecord = library.Texts[element.Index];
                    WriteRecord(GdsRecordTypes.Text, null);
                    WriteCommon(element.Common);
                    WriteLayer(textRecord.Layer);
                    WriteRecord(GdsRecordTypes.TextType, w => w.Write(textRecord.TextType));
                    if (textRecord.Presentation.HasValue) WriteRecord(GdsRecordTypes.Presentation, w => WritePresentationPacked(w, textRecord.Presentation.Value));

                    if (textRecord.PathType is { } pt)
                        WritePathType(pt);
                    if (textRecord.Width is { } w)
                        WriteWidth(w);
                    WriteStrans(textRecord.Strans);
                    WriteRecord(GdsRecordTypes.Xy, w => { WritePoint(w, textRecord.Origin); });
                    WriteRecord(GdsRecordTypes.String, w => WriteGdsString(w, textRecord.Text));
                    break;
                }
                default:
                    throw new InvalidDataException($"Unsupported element kind for writer: {element.Kind}");
            }

            if (propertiesByElement.TryGetValue(element.Index, out var props))
                foreach (var property in props)
                {
                    WriteRecord(GdsRecordTypes.PropertyAttribute, w => w.Write(property.Attribute));
                    WriteRecord(GdsRecordTypes.PropertyValue, w => WriteGdsString(w, property.Value));
                }

            WriteRecord(GdsRecordTypes.EndElement, null);
        }

        WriteRecord(GdsRecordTypes.EndStruct, null);
    }

    private void WriteCommon(GdsElementCommon common)
    {
        if (common.ExternalData.HasValue || common.TemplateData.HasValue)
            WriteRecord(GdsRecordTypes.ElementFlags, w =>
            {
                short payload = 0;
                if (common.ExternalData.GetValueOrDefault()) payload |= 0b10;
                if (common.TemplateData.GetValueOrDefault()) payload |= 0b1;
                w.Write(payload);
            });

        if (common.PlexNumber.HasValue)
        {
            WriteRecord(GdsRecordTypes.Plex, w => w.Write(common.PlexNumber.Value));
        }
    }

    private void WriteLayer(short layer)
        => WriteRecord(GdsRecordTypes.Layer, w => w.Write(layer));

    private void WriteDataType(short dataType)
        => WriteRecord(GdsRecordTypes.DataType, w => w.Write(dataType));

    private void WriteWidth(int width)
        => WriteRecord(GdsRecordTypes.Width, w => w.Write(width));

    private void WriteXyFromStore(long vertexOffset, int vertexCount, IGdsVertexStoreReader vertexReader)
    {
        var arr = ArrayPool<GdsPoint>.Shared.Rent(vertexCount);
        try
        {
            var got = vertexReader.Read(vertexOffset, arr.AsSpan(0, vertexCount));
            if (got != vertexCount)
                throw new InvalidDataException($"Vertex store returned {got} points, expected {vertexCount} for shape at offset {vertexOffset}.");

            WriteRecord(GdsRecordTypes.Xy,
                w =>
                {
                    var asBytes = MemoryMarshal.AsBytes(arr.AsSpan(0, vertexCount));
                    var ints = MemoryMarshal.Cast<byte, int>(asBytes);
                    for (var i = 0; i < ints.Length; i++)
                        ints[i] = BinaryPrimitives.ReverseEndianness(ints[i]);
                    w.Write(asBytes);
                });
        }
        finally
        {
            ArrayPool<GdsPoint>.Shared.Return(arr);
        }
    }

    private void WriteStrans(GdsStransInfo? val)
    {
        if (!val.HasValue) return;
        var s = val.Value;

        WriteRecord(GdsRecordTypes.Strans, w =>
        {
            ushort flags = 0;
            if (s.Reflection) flags |= 0b10000000_00000000;
            if (s.AbsoluteAngle) flags |= 0b100;
            if (s.AbsoluteMagnification) flags |= 0b10;
            w.Write(flags);
        });

        if (s.Magnification.HasValue) WriteRecord(GdsRecordTypes.Magnification, w => w.Write(s.Magnification.Value));
        if (s.Angle.HasValue) WriteRecord(GdsRecordTypes.Angle, w => w.Write(s.Angle.Value));
    }

    private static void WriteTimestampPair(BufferedGdsBinaryWriter w, DateTime a, DateTime b)
    {
        WriteTimestamp(w, a);
        WriteTimestamp(w, b);
    }

    private static void WriteTimestamp(BufferedGdsBinaryWriter w, DateTime dt)
    {
        w.Write((short)dt.Year);
        w.Write((short)dt.Month);
        w.Write((short)dt.Day);
        w.Write((short)dt.Hour);
        w.Write((short)dt.Minute);
        w.Write((short)dt.Second);
    }

    private static void WritePoint(BufferedGdsBinaryWriter w, GdsPoint p)
    {
        w.Write(p.X);
        w.Write(p.Y);
    }

    private static void WritePresentationPacked(BufferedGdsBinaryWriter w, PresentationInfo p)
    {
        ushort packed = 0;
        packed |= (ushort)((p.Font & 0b11) << 4);
        packed |= (ushort)((p.VerticalJustification & 0b11) << 2);
        packed |= (ushort)(p.HorizontalJustification & 0b11);
        w.Write(packed);
    }

    private static void WriteGdsString(BufferedGdsBinaryWriter w, string s)
    {
        var bytes = Encoding.ASCII.GetBytes(s);
        w.Write(bytes);
        if ((bytes.Length & 1) == 1)
            w.Write((byte)0);
    }

    private static void WriteFixedString(BufferedGdsBinaryWriter w, string s, int length)
    {
        var bytes = Encoding.ASCII.GetBytes(s);
        if (bytes.Length > length) Array.Resize(ref bytes, length);
        w.Write(bytes);
        for (var i = bytes.Length; i < length; i++)
            w.Write(0);
    }

    private void WritePathType(GdsPathType pathType)
    {
        WriteRecord(GdsRecordTypes.PathType, w => w.Write((short)pathType));
    }


    private void WriteRecord(ushort code, Action<BufferedGdsBinaryWriter>? payloadWriter)
    {
        _writer.Reset();
        _writer.Write((ushort)0); // Placeholder for length
        _writer.Write(code);

        if (payloadWriter != null)
            payloadWriter(_writer);

        // writeback length in buffer and flush
        var length = (ushort)_writer.BytesWritten;
        var span = _buffer.WrittenSpanMutable;
        BinaryPrimitives.WriteUInt16BigEndian(span[..2], length);

        _output.Write(span);
    }
}