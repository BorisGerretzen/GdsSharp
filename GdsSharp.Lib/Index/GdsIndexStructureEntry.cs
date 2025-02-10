namespace GdsSharp.Lib.Index;

public class GdsIndexStructureEntry
{
    public readonly string Name;
    public readonly long StartOffset;
    public readonly long EndOffset;
    public readonly IReadOnlyList<GdsIndexElementEntry> Elements;

    public GdsIndexStructureEntry(string name, long startOffset, long endOffset, IReadOnlyList<GdsIndexElementEntry> elements)
    {
        Name = name;
        StartOffset = startOffset;
        EndOffset = endOffset;
        Elements = elements;
    }
}