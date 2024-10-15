using GdsSharp.Lib.NonTerminals.Elements;

namespace GdsSharp.Lib.NonTerminals.Abstractions;

public interface IGdsElement
{
    bool ExternalData { get; set; }
    bool TemplateData { get; set; }
    int PlexNumber { get; set; }

    /// <summary>
    /// Materializes the children of the element.
    /// In practice only used for <see cref="GdsBoundaryElement"/> to materialize the points.
    /// </summary>
    virtual void Materialize()
    {
    }
}