using System.Buffers;
using System.Text;
using GdsSharp.Lib.Binary;
using GdsSharp.Lib.InternalDb;
using GdsSharp.Lib.InternalDb.Builder;
using GdsSharp.Lib.InternalDb.VertexStore;
using GdsSharp.Lib.Lexing;
using GdsSharp.Lib.Old.NonTerminals.Enum;
using GdsSharp.Lib.Parsing.Models;

namespace GdsSharp.Lib.Writing;

public sealed class NewGdsWriter(Stream stream)
{
    private readonly Stream _stream = stream ?? throw new ArgumentNullException(nameof(stream));
    private readonly GdsBinaryWriter _writer = new(stream);
    
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

        for (var i = 0; i < library.Structures.Count; i++)
        {
            var s = library.Structures[i];
            WriteStructure(library, s, vertexReader, i);
        }

        WriteRecord(GdsRecordTypes.EndLibrary, payloadWriter: null);
    }

    private void WriteStructure(GdsLibrary library, GdsStructure structure, IGdsVertexStoreReader vertexReader, int cellId)
    {
        var info = structure.Info;

        WriteRecord(GdsRecordTypes.BeginStruct, w => { WriteTimestampPair(w, info.CreationTime, info.ModificationTime); });
        WriteRecord(GdsRecordTypes.StructName, w => { WriteGdsString(w, info.Name); });

        // Write non-text first, then refs then text
        // foreach (var shape in library.ShapeRecords.Where(r => r.Cell.Id == cellId && r.Kind != ShapeKind.Text))
        // {
        //     WriteShape(shape, vertexReader);
        // }
        //
        // foreach (var rr in library.StructureReferences.Where(r => r.Parent.Id == cellId))
        // {
        //     WriteSref(rr);
        // }
        //
        // foreach (var ar in library.ArrayReferences.Where(r => r.Parent.Id == cellId))
        // {
        //     WriteAref(ar);
        // }
        //
        // foreach (var shape in library.ShapeRecords.Where(r => r.Cell.Id == cellId && r.Kind == ShapeKind.Text))
        // {
        //     WriteText(library, shape);
        // }

        WriteRecord(GdsRecordTypes.EndStruct, payloadWriter: null);
    }

    private void WriteShape(ShapeRecord shape, IGdsVertexStoreReader vertexReader)
    {
        switch (shape.Kind)
        {
            case ShapeKind.Boundary:
                WriteRecord(GdsRecordTypes.Boundary, null);
                WriteLayer(shape.Layer);
                WriteDataType(shape.DataType);
                break;

            case ShapeKind.Path:
                WriteRecord(GdsRecordTypes.Path, null);
                WriteLayer(shape.Layer);
                WriteDataType(shape.DataType);
                if(shape.PathType is {} pt)
                    WritePathType(pt);
                if (shape.Width is { } w)
                    WriteWidth(w);
                break;

            case ShapeKind.Node:
                WriteRecord(GdsRecordTypes.Node, null);
                WriteLayer(shape.Layer);
                WriteRecord(GdsRecordTypes.NodeType, w => w.Write(shape.DataType)); 
                break;
            
            case ShapeKind.Text:
            default:
                throw new InvalidDataException($"Unsupported shape kind for writer: {shape.Kind}");
        }

        WriteXyFromStore(shape, vertexReader);
        WriteRecord(GdsRecordTypes.EndElement, null);
    }

    // private void WriteText(GdsLibrary library, ShapeRecord textShape)
    // {
    //     TextRecord textRecord = null!;//FindTextRecord(library, textShape);
    //
    //     WriteRecord(GdsRecordTypes.Text, null);
    //
    //     WriteLayer(textShape.Layer);
    //     WriteRecord(GdsRecordTypes.TextType, w => w.Write(textShape.DataType));
    //
    //     if (textRecord.Presentation.HasValue)
    //     {
    //         WriteRecord(GdsRecordTypes.Presentation, w => { WritePresentationPacked(w, textRecord.Presentation.Value); });
    //     }
    //
    //     if (textRecord.PathType is { } pt)
    //         WriteRecord(GdsRecordTypes.PathType, w => w.Write((short)pt));
    //
    //     if (textShape.Width.HasValue)
    //         WriteWidth(textShape.Width.Value);
    //
    //     if (textRecord.Strans.HasValue)
    //         WriteStrans(textRecord.Strans.Value);
    //
    //     WriteRecord(GdsRecordTypes.Xy, w =>
    //     {
    //         w.Write(textRecord.Origin.X);
    //         w.Write(textRecord.Origin.Y);
    //     });
    //
    //
    //     WriteRecord(GdsRecordTypes.String, w => WriteGdsString(w, textRecord.Text));
    //
    //     // TODO: WriteProperties(library, textShape);
    //     WriteRecord(GdsRecordTypes.EndElement, null);
    // }

    private void WriteSref(GdsStructureReference rr)
    {
        WriteRecord(GdsRecordTypes.StructureReference, null);

        WriteRecord(GdsRecordTypes.StructureName, w => WriteGdsString(w, rr.TargetName));

        WriteStrans(rr.Transform.Strans);

        // XY: one origin point
        WriteRecord(GdsRecordTypes.Xy, w =>
        {
            w.Write(rr.Transform.Origin.X);
            w.Write(rr.Transform.Origin.Y);
        });

        WriteRecord(GdsRecordTypes.EndElement, null);
    }

    private void WriteAref(GdsArrayReference ar)
    {
        WriteRecord(GdsRecordTypes.ArrayReference, null);

        WriteRecord(GdsRecordTypes.StructureName, w => WriteGdsString(w, ar.TargetName));

        WriteStrans(ar.Transform.Strans);

        WriteRecord(GdsRecordTypes.ColumnRow, w =>
        {
            w.Write(ar.Columns);
            w.Write(ar.Rows);
        });

        // XY: 3 points (origin, point on col axis, point on row axis)
        WriteRecord(GdsRecordTypes.Xy, w =>
        {
            WritePoint(w, ar.Transform.Origin);
            WritePoint(w, ar.ColumnVector);
            WritePoint(w, ar.RowVector);
        });

        WriteRecord(GdsRecordTypes.EndElement, null);
    }

    private void WriteLayer(short layer)
        => WriteRecord(GdsRecordTypes.Layer, w => w.Write(layer));

    private void WriteDataType(short dataType)
        => WriteRecord(GdsRecordTypes.DataType, w => w.Write(dataType));

    private void WriteWidth(int width)
        => WriteRecord(GdsRecordTypes.Width, w => w.Write(width));

    private void WriteXyFromStore(ShapeRecord shape, IGdsVertexStoreReader vertexReader)
    {
        var n = shape.VertexCount;
        var rented = ArrayPool<GdsPoint>.Shared.Rent(n);

        try
        {
            // Fill from store using a span (fine here, not in the lambda)
            var got = vertexReader.Read(shape.VertexOffset, rented.AsSpan(0, n));
            if (got != n)
                throw new InvalidDataException($"Vertex store returned {got} points, expected {n} for shape at offset {shape.VertexOffset}.");

            // Capture only heap stuff in the lambda
            var arr = rented;
            var count = n;

            WriteRecord(GdsRecordTypes.Xy, w =>
            {
                for (var i = 0; i < count; i++)
                {
                    var p = arr[i];
                    w.Write(p.X);
                    w.Write(p.Y);
                }
            });
        }
        finally
        {
            ArrayPool<GdsPoint>.Shared.Return(rented);
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

    private static void WriteTimestampPair(GdsBinaryWriter w, DateTime a, DateTime b)
    {
        WriteTimestamp(w, a);
        WriteTimestamp(w, b);
    }

    private static void WriteTimestamp(GdsBinaryWriter w, DateTime dt)
    {
        var u = dt.Kind == DateTimeKind.Utc ? dt : dt.ToUniversalTime();

        w.Write((short)u.Year);
        w.Write((short)u.Month);
        w.Write((short)u.Day);
        w.Write((short)u.Hour);
        w.Write((short)u.Minute);
        w.Write((short)u.Second);
    }

    private static void WritePoint(GdsBinaryWriter w, GdsPoint p)
    {
        w.Write(p.X);
        w.Write(p.Y);
    }

    private static void WritePresentationPacked(GdsBinaryWriter w, PresentationInfo p)
    {
        ushort packed = 0;
        packed |= (ushort)((p.Font & 0b11) << 4);
        packed |= (ushort)((p.VerticalJustification & 0b11) << 2);
        packed |= (ushort)(p.HorizontalJustification & 0b11);
        w.Write(packed);
    }

    private static void WriteGdsString(GdsBinaryWriter w, string s)
    {
        var bytes = Encoding.ASCII.GetBytes(s);
        w.Write(bytes);
        if ((bytes.Length & 1) == 1)
            w.Write((byte)0);
    }

    private static void WriteFixedString(GdsBinaryWriter w, string s, int length)
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

    private void WriteRecord(ushort code, Action<GdsBinaryWriter>? payloadWriter)
    {
        byte[] payload;
        int payloadLen;

        if (payloadWriter is null)
        {
            payload = [];
            payloadLen = 0;
        }
        else
        {
            using var ms = new MemoryStream();
            var temp = new GdsBinaryWriter(ms);
            payloadWriter(temp);
            payload = ms.ToArray();
            payloadLen = payload.Length;
        }

        var recordLen = checked((ushort)(4 + payloadLen));
        _writer.Write(recordLen);
        _writer.Write(code);

        if (payloadLen > 0)
            _stream.Write(payload, 0, payloadLen);
    }

    // private static TextRecord FindTextRecord(GdsLibrary library, ShapeRecord textShape)
    // {
    //     return new TextRecord(2);
    //     // return library.TextRecords[textShape.Shape.Id];
    // }
}