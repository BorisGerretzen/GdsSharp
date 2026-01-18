using GdsSharp.Lib.Library.BoundingBox;
using GdsSharp.Lib.Reading;
using GdsSharp.Lib.Reading.Models;

namespace GdsSharp.Lib.Library.Builder;

public readonly record struct ElementRecord(GdsElementCommon? Common, GdsElementKind Kind, int Index, GdsBoundingBox? BoundingBox, int Layer);