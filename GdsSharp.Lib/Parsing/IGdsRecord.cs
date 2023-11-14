namespace GdsSharp.Lib.Parsing;

public interface IGdsRecord
{
    ushort Code { get; }
    int GetLength();
}