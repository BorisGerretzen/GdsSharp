using GdsSharp.Lib.Reading.Models;

namespace GdsSharp.Lib.InternalDb;

public record struct GdsTransform(
    GdsStransInfo? Strans,
    GdsPoint Origin
);