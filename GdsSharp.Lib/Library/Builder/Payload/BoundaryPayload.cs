namespace GdsSharp.Lib.Library.Builder.Payload;

public readonly record struct BoundaryPayload(short Layer, short DataType, long VertexOffset, int VertexCount);