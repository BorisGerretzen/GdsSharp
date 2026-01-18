using GdsSharp.Lib.Library.BoundingBox;
using GdsSharp.Lib.Library.Views.Elements;
using GdsSharp.Lib.Reading;

namespace GdsSharp.Lib.Library.Views;

public readonly struct ElementView
{
    public readonly int ElementIndex;
    public GdsElementKind Kind => _library.Elements[ElementIndex].Kind;
    public GdsBoundingBox? BoundingBox => _library.Elements[ElementIndex].BoundingBox;

    /// <summary>
    /// Layer number of the element, note that StructureReference and ArrayReference do not have layers and will return -1.
    /// </summary>
    public int Layer => _library.Elements[ElementIndex].Layer;

    private readonly GdsLibrary _library;

    internal ElementView(GdsLibrary library, int elementIndex)
    {
        _library = library;
        ElementIndex = elementIndex;
    }

    public ArrayReferenceView AsArrayReference() => new(_library, ElementIndex);
    public BoundaryView AsBoundary() => new(_library, ElementIndex);
    public BoxView AsBox() => new(_library, ElementIndex);
    public NodeView AsNode() => new(_library, ElementIndex);
    public PathView AsPath() => new(_library, ElementIndex);
    public StructureReferenceView AsStructureReference() => new(_library, ElementIndex);
    public TextView AsText() => new(_library, ElementIndex);
}