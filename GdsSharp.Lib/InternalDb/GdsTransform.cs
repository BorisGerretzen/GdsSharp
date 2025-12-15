using GdsSharp.Lib.Parsing.Models;

namespace GdsSharp.Lib.InternalDb;

public record struct GdsTransform(
    GdsStransInfo Strans,
    GdsPoint Origin
);