using GdsSharp.Lib.Reading.Models;

namespace GdsSharp.Lib.Library.Views.Elements;

public readonly struct ArrayReferenceView
{
    private readonly GdsLibrary _library;
    private readonly int _arefIndex;

    public readonly int ElementIndex;
    public int ParentStructureIndex => _library.ArrayReferences[_arefIndex].Parent.Id;
    public string TargetName => _library.ArrayReferences[_arefIndex].TargetName;
    public GdsStransInfo? Strans => _library.ArrayReferences[_arefIndex].Strans;
    public int Rows => _library.ArrayReferences[_arefIndex].Rows;
    public int Columns => _library.ArrayReferences[_arefIndex].Columns;
    public GdsPoint RowVector => _library.ArrayReferences[_arefIndex].RowVector;
    public GdsPoint ColumnVector => _library.ArrayReferences[_arefIndex].ColumnVector;
    public GdsPoint Origin => _library.ArrayReferences[_arefIndex].Origin;

    internal ArrayReferenceView(GdsLibrary library, int elementIndex)
    {
        _library = library;
        ElementIndex = elementIndex;
        _arefIndex = library.Elements[elementIndex].Index;
    }
}