using GdsSharp.Lib.Terminals.Records;

namespace GdsSharp.Lib.Index;

public readonly struct GdsIndexElementEntry
{
    public readonly long StartOffset;
    public readonly long EndOffset;
    public readonly GdsRecordNoDataType ElementType;

    public GdsIndexElementEntry(long startOffset, long endOffset, GdsRecordNoDataType elementType)
    {
        StartOffset = startOffset;
        EndOffset = endOffset;
        ElementType = elementType;
    }
}