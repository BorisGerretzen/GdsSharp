using GdsSharp.Lib.NonTerminals.Elements;

namespace GdsSharp.Lib.NonTerminals;

public class GdsStructure
{
    public required string Name { get; set; }
    public DateTime CreationTime { get; set; } = DateTime.Now;
    public DateTime ModificationTime { get; set; } = DateTime.Now;
    public IEnumerable<GdsElement> Elements { get; set; } = new List<GdsElement>();
}