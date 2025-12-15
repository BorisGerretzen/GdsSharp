using GdsSharp.Lib.Binary;

namespace GdsSharp.Lib.Old.Terminals.Abstractions;

public interface IGdsReadableRecord : IGdsRecord
{
    void Read(GdsBinaryReader reader, GdsHeader header);
}