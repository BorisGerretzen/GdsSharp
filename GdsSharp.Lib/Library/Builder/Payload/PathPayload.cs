using GdsSharp.Lib.Obsolete.NonTerminals.Enum;

namespace GdsSharp.Lib.Library;

public readonly record struct PathPayload(short Layer, short DataType, GdsPathType? PathType, int? Width, long VertexOffset, int VertexCount);