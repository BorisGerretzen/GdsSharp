using GdsSharp.Lib.NonTerminals.Elements;

namespace GdsSharp.Lib.NonTerminals;

public class GdsStructure
{
    public delegate GdsStructure? StructureProvider(string structureName);

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

        Elements = elements;
    }

    /// <summary>
    ///     Calculates the bounding box of the structure.
    /// </summary>
    /// <param name="structureProvider">Maps structure names to structure objects. <see cref="GdsFile.GetStructure" /></param>
    /// <returns>Bounding box of the structure.</returns>
    public GdsBoundingBox GetBoundingBox(StructureProvider structureProvider)
    {
        var boundingBoxes = Elements.Select(e => e.Element.GetBoundingBox(structureProvider));
        return new GdsBoundingBox(boundingBoxes);
    }
}