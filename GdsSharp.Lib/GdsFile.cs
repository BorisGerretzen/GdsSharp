using GdsSharp.Lib.Parsing.Abstractions;

namespace GdsSharp.Lib;

public class GdsFile
{
    public required IList<IGdsRecord> Records { get; init; }
}