using GdsSharp.Lib.Parsing.Models;

namespace GdsSharp.Lib.InternalDb;

public readonly record struct GdsStructure(
    GdsStructureInfo Info,
    GdsBoundingBox? BoundingBox = null
);