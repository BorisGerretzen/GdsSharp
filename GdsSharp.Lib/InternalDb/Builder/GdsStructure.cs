using GdsSharp.Lib.InternalDb.BoundingBox;
using GdsSharp.Lib.Parsing.Models;

namespace GdsSharp.Lib.InternalDb.Builder;

public readonly record struct GdsStructure(
    GdsStructureInfo Info,
    int ElementStartIndex,
    int ElementCount,
    GdsBoundingBox? BoundingBox = null
);