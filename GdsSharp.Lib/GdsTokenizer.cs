using GdsSharp.Lib.Terminals;
using GdsSharp.Lib.Terminals.Abstractions;

namespace GdsSharp.Lib;

public class GdsTokenizer : GdsStreamOperator
{
    private readonly GdsBinaryReader _reader;

    public GdsTokenizer(Stream stream)
    {
        _reader = new GdsBinaryReader(stream);
    }

    /// <summary>
    ///     Tokenizes the stream.
    /// </summary>
    /// <param name="token">Cancellation token.</param>
    /// <returns>IEnumerable of the found tokens.</returns>
    /// <exception cref="InvalidOperationException">If an invalid record code is found or lengths mismatch.</exception>
    public IEnumerable<IGdsRecord> Tokenize(CancellationToken token = default)
    {
        while (!token.IsCancellationRequested && _reader.BaseStream.Position < _reader.BaseStream.Length)
        {
            var currentHeader = new GdsHeader();
            ((IGdsSimpleRead)currentHeader).Read(_reader, currentHeader);

            // Stop when padding is reached
            if (currentHeader is { Code: 0, Length: 0 }) yield break;

            // Get record
            if (!Activators.TryGetValue(currentHeader.Code, out var activator))
                throw new InvalidOperationException(
                    $"Could not find activator for code 0x{currentHeader.Code:X} ({currentHeader.Code}) at position 0x{_reader.BaseStream.Position:X} ({_reader.BaseStream.Position})");
            var record = activator.Invoke();

            if (record is IGdsReadableRecord readableRecord) readableRecord.Read(_reader, currentHeader);
            if (record.GetLength() != currentHeader.NumToRead)
                throw new InvalidOperationException(
                    $"Record length mismatch at position 0x{_reader.BaseStream.Position:X} ({_reader.BaseStream.Position}), expected {currentHeader.NumToRead}, got {record.GetLength()}");

            yield return record;
        }
    }
}