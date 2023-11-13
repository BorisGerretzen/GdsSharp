namespace GdsSharp.Lib.Parsing.Tokens;

public struct GdsRecordNoData : IGdsRecord
{
    public GdsRecordNoDataType Type { get; init; }
}

public enum GdsRecordNoDataType
{
    EndLib,
    EndStr,
    Boundary,
    Path,
    Sref,
    Aref,
    Text,
    EndEl,
    Node,
    Box,
    EndMasks
}