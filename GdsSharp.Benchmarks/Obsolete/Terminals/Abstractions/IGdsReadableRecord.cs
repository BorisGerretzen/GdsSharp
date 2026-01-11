using GdsSharp.Lib.Obsolete.Binary;

namespace GdsSharp.Lib.Obsolete.Terminals.Abstractions;

public interface IGdsReadableRecord : IGdsRecord
{
    void Read(GdsBinaryReader reader, GdsHeader header);
}