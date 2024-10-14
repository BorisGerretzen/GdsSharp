using GdsSharp.Lib.Terminals;
using GdsSharp.Lib.Terminals.Abstractions;

namespace GdsSharp.Lib;

public record GdsTokenReference(GdsHeader Header, IGdsRecord Record, long Offset);