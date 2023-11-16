namespace GdsSharp.Lib.Parsing.Abstractions;

public interface IGdsRecord
{
    ushort Code { get; }
    ushort GetLength();
}