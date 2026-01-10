namespace GdsSharp.Lib.Library;

public readonly record struct BoxPayload(short Layer, short BoxType, long VertexOffset, int VertexCount);