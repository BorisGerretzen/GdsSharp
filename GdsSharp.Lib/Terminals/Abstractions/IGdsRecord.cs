namespace GdsSharp.Lib.Terminals.Abstractions;

public interface IGdsRecord
{
    ushort Code { get; }
    ushort GetLength();
}