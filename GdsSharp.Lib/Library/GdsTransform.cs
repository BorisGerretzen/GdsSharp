using GdsSharp.Lib.Reading.Models;

namespace GdsSharp.Lib.Library;

public record struct GdsTransform(
    GdsStransInfo? Strans,
    GdsPoint Origin
);