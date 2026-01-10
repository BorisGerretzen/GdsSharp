using GdsSharp.Lib.Library.BoundingBox;
using GdsSharp.Lib.Reading.Models;

namespace GdsSharp.Lib.Library.Builder;

public readonly record struct GdsStructure(
    GdsStructureInfo Info,
    int ElementStartIndex,
    int ElementCount,
    GdsBoundingBox? BoundingBox = null
);