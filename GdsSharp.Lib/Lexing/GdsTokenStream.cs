using System.Collections;
using GdsSharp.Lib.Abstractions;
using GdsSharp.Lib.Binary;
using GdsSharp.Lib.Terminals;
using GdsSharp.Lib.Terminals.Abstractions;
using GdsSharp.Lib.Terminals.Records;

namespace GdsSharp.Lib.Lexing;

public class GdsTokenStream : GdsStreamOperator, IDisposable, IEnumerable<IGdsRecord>
{
    private readonly GdsBinaryReader _reader;

    public GdsTokenStream(Stream stream)
    {
        _reader = new GdsBinaryReader(stream);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _reader.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc />
    public IEnumerator<IGdsRecord> GetEnumerator()
    {
        GdsTokenReference? element;
        while ((element = Read()) != null) yield return element.Record;
    }

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    /// <summary>
    /// Peeks at the next item in the queue.
    /// </summary>
    /// <returns>Next item in the queue.</returns>
    /// <exception cref="InvalidOperationException">If queue is empty.</exception>
    public GdsTokenReference Peek()
    {
        var position = _reader.BaseStream.Position;
        try
        {
            return Dequeue();
        }
        finally
        {
            SetPosition(position);
        }
    }

    /// <summary>
    /// Dequeues the next item in the queue.
    /// </summary>
    /// <returns>Next item in the queue.</returns>
    /// <exception cref="InvalidOperationException">If queue is empty.</exception>
    public GdsTokenReference Dequeue()
    {
        var element = Read();
        if (element is null) throw new InvalidOperationException("No more items in the queue.");
        return element;
    }

    /// <summary>
    /// Sets the position of the reader.
    /// </summary>
    /// <param name="position">Position in the stream.</param>
    /// <returns>Old position.</returns>
    public long SetPosition(long position)
    {
        var oldPosition = _reader.BaseStream.Position;
        _reader.BaseStream.Position = position;
        return oldPosition;
    }

    private GdsTokenReference? Read()
    {
        var pos = _reader.BaseStream.Position;
        if (pos == _reader.BaseStream.Length) return null;

        var header = new GdsHeader();
        ((IGdsSimpleRead)header).Read(_reader, header);

        // Stop when padding is reached
        if (header is { Code: 0, Length: 0 }) return null;

        // Get record
        if (!Activators.TryGetValue(header.Code, out var activator))
            throw new InvalidOperationException(
                $"Could not find activator for code 0x{header.Code:X} ({header.Code}) at position 0x{_reader.BaseStream.Position:X} ({_reader.BaseStream.Position})");
        var record = activator.Invoke();

        switch (record)
        {
            case GdsRecordXy xy:
                xy.NumPoints = header.NumToRead / 8;
                xy.Coordinates = ReadGdsPoints(_reader.BaseStream.Position, header);
                _reader.BaseStream.Position += header.NumToRead;
                break;
            case IGdsReadableRecord readableRecord:
                readableRecord.Read(_reader, header);
                break;
        }

        if (record.GetLength() != header.NumToRead)
            throw new InvalidOperationException(
                $"Record length mismatch at position 0x{_reader.BaseStream.Position:X} ({_reader.BaseStream.Position}), expected {header.NumToRead}, got {record.GetLength()}");

        return new GdsTokenReference(header, record, pos);
    }

    private IEnumerable<GdsPoint> ReadGdsPoints(long offset, GdsHeader header)
    {
        var oldPosition = _reader.BaseStream.Position;
        try
        {
            _reader.BaseStream.Position = offset;
            for (var i = 0; i < header.NumToRead / 8; i++)
                yield return new GdsPoint(_reader.ReadInt32(), _reader.ReadInt32());
        }
        finally
        {
            _reader.BaseStream.Position = oldPosition;
        }
    }
}