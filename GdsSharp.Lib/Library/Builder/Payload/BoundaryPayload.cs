namespace GdsSharp.Lib.Library;

public readonly record struct BoundaryPayload(short Layer, short DataType, long VertexOffset, int VertexCount);