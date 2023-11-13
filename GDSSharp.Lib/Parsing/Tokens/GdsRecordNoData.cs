namespace GdsSharp.Lib.Parsing.Tokens;

public struct GdsRecordNoData : IGdsRecord
{
    public bool CanRead => false;
    public GdsRecordNoDataType Type { get; init; }

    public void Read(GdsBinaryReader reader, GdsHeader header)
    {
        throw new InvalidOperationException($"Record type {nameof(GdsRecordNoData)} does not have data.");
    }
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