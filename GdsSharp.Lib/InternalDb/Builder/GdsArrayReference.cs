namespace GdsSharp.Lib.InternalDb.Builder;

public record struct GdsArrayReference(CellId Parent, string TargetName, GdsTransform Transform, int Rows, int Columns, GdsPoint RowVector, GdsPoint ColumnVector);