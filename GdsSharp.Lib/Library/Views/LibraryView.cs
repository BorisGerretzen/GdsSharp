using GdsSharp.Lib.Reading.Models;

namespace GdsSharp.Lib.Library.Views;

public readonly struct LibraryView
{
    private readonly GdsLibrary _library;
    public GdsLibraryInfo Info => _library.Info;
    public StructureCollection Structures => new(_library);

    public bool TryGetStructure(string name, out StructureView structure)
    {
        if (!_library.TryGetStructureIndex(name, out var index))
        {
            structure = default;
            return false;
        }

        structure = new StructureView(_library, index);
        return true;
    }

    internal LibraryView(GdsLibrary library)
    {
        _library = library;
    }
}