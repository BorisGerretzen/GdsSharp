using GdsSharp.Lib.Binary;

namespace GdsSharp.Lib.Old.Terminals.Abstractions;

public interface IGdsWriteableRecord : IGdsRecord
{
    void Write(GdsBinaryWriter writer);
}