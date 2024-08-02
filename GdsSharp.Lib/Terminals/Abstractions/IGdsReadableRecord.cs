using GdsSharp.Lib.Binary;

namespace GdsSharp.Lib.Terminals.Abstractions;

public interface IGdsReadableRecord : IGdsRecord
{
    void Read(GdsBinaryReader reader, GdsHeader header);
}