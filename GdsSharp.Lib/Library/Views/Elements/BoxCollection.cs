using GdsSharp.Lib.Reading;

namespace GdsSharp.Lib.Library.Views.Elements;

public readonly struct BoxCollection
{
    public readonly int StructureIndex;
    private readonly GdsLibrary _library;
    
    internal BoxCollection(GdsLibrary library, int structureIndex)
    {
        _library = library;
        StructureIndex = structureIndex;
    }
    
    public Enumerator GetEnumerator() => new(_library, StructureIndex);
    
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

        public BoxView Current => new(_lib, _elementIndex + _elementsStartIndex);

        public bool MoveNext()
        {
            while (++_elementIndex < _elementsCount)
            {
                if (_lib.Elements[_elementIndex + _elementsStartIndex].Kind == GdsElementKind.Box)
                    return true;
            }

            return false;
        }
    }
}
