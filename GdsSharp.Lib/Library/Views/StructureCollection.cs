namespace GdsSharp.Lib.Library.Views;

public readonly struct StructureCollection
{
    public int Count => _library.Structures.Length;
    public StructureView this[int index] => new(_library, index);
    public StructureView this[string name] => !TryGetStructure(name, out var structure) ? throw new KeyNotFoundException($"Structure with name '{name}' not found.") : structure;

    private readonly GdsLibrary _library;

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

    internal StructureCollection(GdsLibrary library)
    {
        _library = library;
    }

    public Enumerator GetEnumerator()
    {
        return new Enumerator(_library);
    }

    public struct Enumerator
    {
        private readonly GdsLibrary _lib;
        private int _i;

        internal Enumerator(GdsLibrary lib)
        {
            _lib = lib;
            _i = -1;
        }

        public StructureView Current => new(_lib, _i);

        public bool MoveNext()
        {
            return ++_i < _lib.Structures.Length;
        }
    }
}