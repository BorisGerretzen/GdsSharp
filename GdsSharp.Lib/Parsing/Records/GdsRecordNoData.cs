using GdsSharp.Lib.Parsing.Abstractions;

namespace GdsSharp.Lib.Parsing.Records;

public struct GdsRecordNoData : IGdsWriteableRecord
{
    public required GdsRecordNoDataType Type { get; init; }

    public ushort Code => (ushort)Type;

    public ushort GetLength()
    {
        return 0;
    }

    public void Write(GdsBinaryWriter writer)
    {
    }
}

public enum GdsRecordNoDataType
{
    EndLib = 0x0400,
    EndStr = 0x0700,
    Boundary = 0x0800,
    Path = 0x0900,
    Sref = 0x0A00,
    Aref = 0x0B00,
    Text = 0x0C00,
    EndEl = 0x1100,
    Node = 0x1500,
    Box = 0x2D00,
    EndMasks = 0x3800
}