using GdsSharp.Lib.NonTerminals.Abstractions;

namespace GdsSharp.Lib.NonTerminals.Elements;

public class GdsElement
{
    public required IGdsElement Element { get; set; }
    public List<GdsProperty> Properties { get; set; } = new();
}