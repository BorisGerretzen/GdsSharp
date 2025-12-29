using GdsSharp.Lib.Obsolete.Binary;

namespace GdsSharp.Lib.Obsolete.Terminals.Abstractions;

public interface IGdsWriteableRecord : IGdsRecord
{
    void Write(GdsBinaryWriter writer);
}