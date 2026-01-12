namespace GdsSharp.Lib.Library.Builder.Payload;

public readonly record struct BoxPayload(short Layer, short BoxType, long VertexOffset, int VertexCount);