namespace GdsSharp.Lib.Parsing.Abstractions;

public interface IGdsReadableRecord : IGdsRecord
{
    void Read(GdsBinaryReader reader, GdsHeader header);
}