using GdsSharp.Lib.Library.Builder;
using GdsSharp.Lib.Reading.Models;

namespace GdsSharp.Lib.Library;

public readonly record struct SRefPayload(CellId Parent, string TargetName, GdsStransInfo? Strans, GdsPoint Origin);