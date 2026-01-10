using GdsSharp.Lib.Library.Builder;
using GdsSharp.Lib.Reading.Models;

namespace GdsSharp.Lib.Library;

public readonly record struct ARefPayload(
    CellId Parent,
    string TargetName,
    GdsStransInfo? Strans,
    int Rows,
    int Columns,
    GdsPoint RowVector,
    GdsPoint ColumnVector,
    GdsPoint Origin);