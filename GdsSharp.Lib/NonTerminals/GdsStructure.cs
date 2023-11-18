using GdsSharp.Lib.NonTerminals.Elements;

namespace GdsSharp.Lib.NonTerminals;

public class GdsStructure
{
    public required string Name { get; set; }
    public required DateTime CreationTime { get; set; }
    public required DateTime ModificationTime { get; set; }
    public List<GdsElement> Elements { get; set; } = new();
}