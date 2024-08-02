using GdsSharp.Lib.Binary;

namespace GdsSharp.Lib.Terminals.Abstractions;

public interface IGdsWriteableRecord : IGdsRecord
{
    void Write(GdsBinaryWriter writer);
}