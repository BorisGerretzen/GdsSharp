using GdsSharp.Lib.Old.NonTerminals.Abstractions;

namespace GdsSharp.Lib.Old.NonTerminals.Elements;

public class GdsElement
{
    public required IGdsElement Element { get; set; }
    public List<GdsProperty> Properties { get; set; } = new();
    
    /// <summary>
    /// Materializes the element.
    /// </summary>
    public void Materialize()
    {
        Element.Materialize();
    }
}