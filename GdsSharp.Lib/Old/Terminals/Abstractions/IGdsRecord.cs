namespace GdsSharp.Lib.Old.Terminals.Abstractions;

public interface IGdsRecord
{
    ushort Code { get; }
    ushort GetLength();
}