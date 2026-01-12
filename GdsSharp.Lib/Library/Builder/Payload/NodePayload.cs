namespace GdsSharp.Lib.Library.Builder.Payload;

public readonly record struct NodePayload(short Layer, short NodeType, long VertexOffset, int VertexCount);