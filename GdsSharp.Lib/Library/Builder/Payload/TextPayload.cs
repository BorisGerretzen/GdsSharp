using GdsSharp.Lib.Obsolete.NonTerminals.Enum;
using GdsSharp.Lib.Reading.Models;

namespace GdsSharp.Lib.Library;

public readonly record struct TextPayload(
    short Layer,
    short TextType,
    PresentationInfo? Presentation,
    GdsPathType? PathType,
    int? Width,
    GdsStransInfo? Strans,
    GdsPoint Origin,
    string Text);