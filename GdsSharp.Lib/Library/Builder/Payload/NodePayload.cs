namespace GdsSharp.Lib.Library;

public readonly record struct NodePayload(short Layer, short NodeType, long VertexOffset, int VertexCount);