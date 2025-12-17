using GdsSharp.Lib.InternalDb.BoundingBox;
using GdsSharp.Lib.Parsing.Models;

namespace GdsSharp.Lib.InternalDb.Builder;

public readonly record struct GdsStructure(
    GdsStructureInfo Info,
    GdsBoundingBox? BoundingBox = null
);