namespace GdsSharp.Lib.Parsing.Abstractions;

public interface IGdsWriteableRecord : IGdsRecord
{
    void Write(GdsBinaryWriter writer);
}