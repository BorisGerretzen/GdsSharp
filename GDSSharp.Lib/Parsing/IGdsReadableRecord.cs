using GdsSharp.Lib.Parsing.Tokens;

namespace GdsSharp.Lib.Parsing;

public interface IGdsReadableRecord : IGdsRecord
{
    void Read(GdsBinaryReader reader, GdsHeader header);
}