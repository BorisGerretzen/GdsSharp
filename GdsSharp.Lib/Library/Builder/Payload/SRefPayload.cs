using GdsSharp.Lib.Reading.Models;

namespace GdsSharp.Lib.Library.Builder.Payload;

public readonly record struct SRefPayload(CellId Parent, string TargetName, GdsStransInfo? Strans, GdsPoint Origin);