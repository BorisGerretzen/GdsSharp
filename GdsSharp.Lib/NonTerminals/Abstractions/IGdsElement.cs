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

    /// <summary>
    ///     Calculates the bounding box of the element.
    /// </summary>
    /// <param name="structureProvider">Maps structure names to structure objects. <see cref="GdsFile.GetStructure" /></param>
    /// <returns>The bounding box of the element.</returns>
    GdsBoundingBox GetBoundingBox(GdsStructure.StructureProvider structureProvider);
}