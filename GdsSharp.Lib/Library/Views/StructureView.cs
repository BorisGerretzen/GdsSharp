using GdsSharp.Lib.Library.BoundingBox;
using GdsSharp.Lib.Library.Views.Elements;

namespace GdsSharp.Lib.Library.Views;

public readonly struct StructureView
{
    public readonly int Index;
    public string Name => _library.Structures[Index].Info.Name;
    public GdsBoundingBox? BoundingBox => _library.Structures[Index].BoundingBox;

    public ElementCollection Elements => new(_library, Index);

    private readonly GdsLibrary _library;

    internal StructureView(GdsLibrary library, int index)
    {
        _library = library;
        Index = index;
    }

    public ArrayReferenceCollection ArrayReferences => new(_library, Index);
    public BoundaryCollection Boundaries => new(_library, Index);
    public BoxCollection Boxes => new(_library, Index);
    public NodeCollection Nodes => new(_library, Index);
    public PathCollection Paths => new(_library, Index);
    public StructureReferenceCollection StructureReferences => new(_library, Index);
    public TextCollection Texts => new(_library, Index);
}