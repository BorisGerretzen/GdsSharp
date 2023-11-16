namespace GdsSharp.Lib.Parsing.Tokens;

public class GdsHeader : IGdsSimpleRead, IGdsSimpleWrite
{
    public const int RecordSize = 4;

    /// <summary>
    ///     Total length of the record, including the header itself.
    ///     When setting make sure you include <see cref="RecordSize" />.
    /// </summary>
    public ushort Length { get; init; }

    /// <summary>
    ///     Number of bytes to read after the header.
    /// </summary>
    public ushort NumToRead => (ushort)(Length - RecordSize);

    /// <summary>
    ///     Record type.
    /// </summary>
    public ushort Code { get; init; }

    public ushort GetLength()
    {
        return RecordSize;
    }
}