namespace GdsSharp.Lib.Library.Views;

public readonly struct ElementCollection
{
    public readonly int StructureIndex;
    public readonly int Count;

    private readonly GdsLibrary _library;

    internal ElementCollection(GdsLibrary library, int structureIndex)
    {
        _library = library;
        Count = library.Structures[structureIndex].ElementCount;
        StructureIndex = structureIndex;
    }

    public Enumerator GetEnumerator()
    {
        return new Enumerator(_library, StructureIndex);
    }

    public struct Enumerator
    {
        private readonly GdsLibrary _lib;
        private int _elementIndex;

        private readonly int _elementsStartIndex;
        private readonly int _elementsCount;

        internal Enumerator(GdsLibrary lib, int structureIndex)
        {
            _lib = lib;
            _elementsStartIndex = lib.Structures[structureIndex].ElementStartIndex;
            _elementsCount = lib.Structures[structureIndex].ElementCount;
            _elementIndex = -1;
        }

        public ElementView Current => new(_lib, _elementIndex + _elementsStartIndex);

        public bool MoveNext()
        {
            return ++_elementIndex < _elementsCount;
        }
    }
}