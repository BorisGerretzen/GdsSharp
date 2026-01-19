using GdsSharp.Lib.Library.BoundingBox;
using GdsSharp.Lib.Reading;
using GdsSharp.Lib.Reading.Models;

namespace GdsSharp.Lib.Library.Editing;

internal sealed class StructureDelta
{
    /// <summary>
    ///     Copy-on-write element sequence for this structure. Null until first edit.
    /// </summary>
    public List<ElementKey>? Elements;
}

internal sealed class ElementDelta
{
    public GdsBoundingBox? BoundingBoxOverride;
    public object? PayloadOverride;
    public List<(short Attr, string Value)>? PropertiesOverride;
}

internal sealed class NewElement
{
    public GdsBoundingBox? BoundingBox;
    public GdsElementCommon? Common;
    public GdsElementKind Kind;
    public object Payload = null!;
    public List<(short Attr, string Value)>? Properties;
}