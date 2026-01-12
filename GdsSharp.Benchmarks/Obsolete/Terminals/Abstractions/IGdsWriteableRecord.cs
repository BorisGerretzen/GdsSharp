using GdsSharp.Benchmarks.Obsolete.Binary;

namespace GdsSharp.Benchmarks.Obsolete.Terminals.Abstractions;

public interface IGdsWriteableRecord : IGdsRecord
{
    void Write(GdsBinaryWriter writer);
}