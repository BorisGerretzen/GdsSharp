namespace GdsSharp.Lib.Library.Editing;

internal enum VertexStoreKind : byte
{
    Base = 0,
    Delta = 1
}

internal readonly record struct VertexRef(VertexStoreKind Store, long Offset, int Count);