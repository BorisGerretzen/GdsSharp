using GdsSharp.Benchmarks.Obsolete.Binary;

namespace GdsSharp.Benchmarks.Obsolete.Terminals.Abstractions;

public interface IGdsReadableRecord : IGdsRecord
{
    void Read(GdsBinaryReader reader, GdsHeader header);
}