using GdsSharp.Lib.Parsing.Tokens;

namespace GdsSharp.Lib.Parsing;

public interface IGdsRecord
{
    bool CanRead => true;
    void Read(GdsBinaryReader reader, GdsHeader header);
}