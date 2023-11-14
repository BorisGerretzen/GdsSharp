namespace GdsSharp.Lib.Parsing;

public interface IGdsRecord
{
    ushort Code { get; }
    ushort GetLength();
}