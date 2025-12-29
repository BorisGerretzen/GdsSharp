using GdsSharp.Lib.Obsolete.NonTerminals.Enum;
using GdsSharp.Lib.Reading.Models;

namespace GdsSharp.Lib.InternalDb.Builder;

public readonly record struct TextRecord(
    ShapeId ShapeId,
    string Text,
    PresentationInfo? Presentation,
    GdsPathType? PathType,
    GdsStransInfo? Strans,
    GdsPoint Origin
);