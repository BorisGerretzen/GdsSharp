using GdsSharp.Lib.Old.NonTerminals.Enum;
using GdsSharp.Lib.Parsing.Models;

namespace GdsSharp.Lib.InternalDb.Builder;

public readonly record struct TextRecord(
    ShapeId ShapeId,
    string Text,
    PresentationInfo? Presentation,
    GdsPathType? PathType,
    GdsStransInfo? Strans,
    GdsPoint Origin
);