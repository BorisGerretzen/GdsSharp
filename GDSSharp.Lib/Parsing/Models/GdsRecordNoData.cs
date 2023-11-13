using GdsSharp.Lib.Models.Parsing;

namespace GdsSharp.Lib.Parsing.Models;

public struct GdsRecordNoData : IGdsRecord
{
    public GdsRecordNoDataType Type { get; init; }
}

public enum GdsRecordNoDataType
{
    EndLib,
    EndStr,
    Boundary
}