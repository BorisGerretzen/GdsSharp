using GdsSharp.Lib.Library.Builder;
using GdsSharp.Lib.Reading.Models;

namespace GdsSharp.Lib.Library.Views.Elements;

public readonly struct StructureReferenceView
{
    private readonly GdsLibrary _library;
    private readonly int _srefIndex;
    
    public readonly int ElementIndex;
    public CellId Parent => _library.StructureReferences[_srefIndex].Parent;
    public string TargetName => _library.StructureReferences[_srefIndex].TargetName;
    public GdsStransInfo? Strans => _library.StructureReferences[_srefIndex].Strans;
    public GdsPoint Origin => _library.StructureReferences[_srefIndex].Origin;
    
    internal StructureReferenceView(GdsLibrary library, int elementIndex)
    {
        _library = library;
        ElementIndex = elementIndex;
        _srefIndex = library.Elements[elementIndex].Index;
    }
}
