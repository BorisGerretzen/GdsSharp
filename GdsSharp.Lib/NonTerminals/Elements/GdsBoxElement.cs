using GdsSharp.Lib.NonTerminals.Abstractions;

namespace GdsSharp.Lib.NonTerminals.Elements;

public class GdsBoxElement : IGdsLayeredElement
{
    public required short BoxType { get; set; }

    /// <summary>
    ///     5 points defining the box, note that the first and last point should be the same.
    /// </summary>
    public List<GdsPoint> Points { get; set; } = new();

    public bool ExternalData { get; set; }
    public bool TemplateData { get; set; }
    public int PlexNumber { get; set; }
    public short Layer { get; set; }
}