using GdsSharp.Lib.Reading.Models;

namespace GdsSharp.Lib.Library.Builder.Payload;

public readonly record struct ARefPayload(
    CellId Parent,
    string TargetName,
    GdsStransInfo? Strans,
    int Rows,
    int Columns,
    GdsPoint RowVector,
    GdsPoint ColumnVector,
    GdsPoint Origin);