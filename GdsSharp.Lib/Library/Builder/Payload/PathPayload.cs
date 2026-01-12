using GdsSharp.Lib.Reading.Enum;

namespace GdsSharp.Lib.Library.Builder.Payload;

public readonly record struct PathPayload(short Layer, short DataType, GdsPathType? PathType, int? Width, long VertexOffset, int VertexCount);