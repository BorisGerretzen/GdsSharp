using GdsSharp.Lib.NonTerminals.Elements;

namespace GdsSharp.Lib.NonTerminals;

public class GdsStructure
{
    public required string Name { get; set; }
    public DateTime CreationTime { get; set; } = DateTime.Now;
    public DateTime ModificationTime { get; set; } = DateTime.Now;
    public IEnumerable<GdsElement> Elements { get; set; } = new List<GdsElement>();
    
    /// <summary>
    /// Materializes all elements in the structure.
    /// </summary>
    public void Materialize()
    {
        var elements = Elements.ToList();
        foreach (var element in elements)
        {
            element.Materialize();
        }
    }
}