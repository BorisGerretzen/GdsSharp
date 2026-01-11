using GdsSharp.Lib.Library.BoundingBox;
using GdsSharp.Lib.Reading.Models;

namespace GdsSharp.Lib.Library;

public readonly record struct ElementRecord(GdsElementCommon? Common, ElementKind Kind, int Index, GdsBoundingBox? BoundingBox);