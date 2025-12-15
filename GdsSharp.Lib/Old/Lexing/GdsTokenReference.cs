using GdsSharp.Lib.Old.Terminals;
using GdsSharp.Lib.Old.Terminals.Abstractions;

namespace GdsSharp.Lib.Old.Lexing;

public record GdsTokenReference(GdsHeader Header, IGdsRecord Record, long Offset);