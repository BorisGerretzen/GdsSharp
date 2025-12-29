using GdsSharp.Lib.Obsolete.Terminals;
using GdsSharp.Lib.Obsolete.Terminals.Abstractions;

namespace GdsSharp.Lib.Obsolete.Lexing;

public record GdsTokenReference(GdsHeader Header, IGdsRecord Record, long Offset);