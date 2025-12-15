namespace GdsSharp.Lib.Lexing.Tokens;
public enum GdsTokenKind
{
    NoData,
    Int16,
    Int32,
    String,
    Real8,
    Xy,
    Unknown
}

// public readonly record struct Token(GdsTokenHeader Header, GdsTokenBody Body);
public readonly record struct GdsTokenHeader(ushort Length, ushort Code, long Offset, long PayloadOffset)
{
    public int PayloadLength => Length - 4;
};
// public readonly record struct XyPayload(long DataOffset, int NumPoints);
// public readonly record struct GdsTokenBody(
//     GdsTokenKind Kind,
//     GdsRecordNoDataType NoDataType,
//     int IntValue,
//     string? StringValue,
//     double RealValue,
//     XyPayload XyValue,
//     ushort RawCode,
//     int RawBytes
// )
// {
//     public static GdsTokenBody NoData(GdsRecordNoDataType t)
//         => new(GdsTokenKind.NoData, t, 0, null, 0, default, 0, 0);
//
//     public static GdsTokenBody Int16(int v)
//         => new(GdsTokenKind.Int16, 0, v, null, 0, default, 0, 0);
//
//     public static GdsTokenBody Int32(int v)
//         => new(GdsTokenKind.Int32, 0, v, null, 0, default, 0, 0);
//
//     public static GdsTokenBody String(string s)
//         => new(GdsTokenKind.String, 0, 0, s, 0, default, 0, 0);
//
//     public static GdsTokenBody Real8(double v)
//         => new(GdsTokenKind.Real8, 0, 0, null, v, default, 0, 0);
//
//     public static GdsTokenBody Xy(XyPayload xy)
//         => new(GdsTokenKind.Xy, 0, 0, null, 0, xy, 0, 0);
//
//     public static GdsTokenBody Unknown(ushort code, int bytes)
//         => new(GdsTokenKind.Unknown, 0, 0, null, 0, default, code, bytes);
// }