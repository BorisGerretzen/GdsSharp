namespace GdsSharp.Lib.Obsolete.Terminals.Abstractions;

public interface IGdsRecord
{
    ushort Code { get; }
    ushort GetLength();
}