using GdsSharp.Benchmarks.Obsolete.Terminals;
using GdsSharp.Benchmarks.Obsolete.Terminals.Abstractions;

namespace GdsSharp.Benchmarks.Obsolete.Lexing;

public record GdsTokenReference(GdsHeader Header, IGdsRecord Record, long Offset);