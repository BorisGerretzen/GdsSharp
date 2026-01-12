using GdsSharp.Lib.Reading.Enum;
using GdsSharp.Lib.Reading.Models;

namespace GdsSharp.Lib.Library.Builder.Payload;

public readonly record struct TextPayload(
    short Layer,
    short TextType,
    PresentationInfo? Presentation,
    GdsPathType? PathType,
    int? Width,
    GdsStransInfo? Strans,
    GdsPoint Origin,
    string Text);