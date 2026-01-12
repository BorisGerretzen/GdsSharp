using GdsSharp.Lib.Obsolete.Lexing;
using GdsSharp.Lib.Obsolete.NonTerminals;
using GdsSharp.Lib.Reading.Enum;

namespace GdsSharp.Lib.Obsolete;

public class GdsFile
{
    public short Version { get; set; } = 5;
    public required string LibraryName { get; set; }
    public DateTime LastModificationTime { get; set; } = DateTime.Now;
    public DateTime LastAccessTime { get; set; } = DateTime.Now;
    public List<string> ReferencedLibraries { get; set; } = new();
    public List<string> Fonts { get; set; } = new();
    public string? AttributeDefinitionFile { get; set; }
    public short Generations { get; set; } = 3;
    public required double UserUnits { get; set; }
    public required double PhysicalUnits { get; set; }
    public GdsFormatType FormatType { get; set; } = GdsFormatType.GdsArchive;
    public IEnumerable<GdsStructure> Structures { get; set; } = new List<GdsStructure>();

    public GdsStructure? GetStructure(string name)
    {
        return Structures.FirstOrDefault(s => s.Name == name);
    }

    public static GdsFile From(Stream stream)
    {
        using var tokenStream = new ObsGdsTokenStream(stream);
        var parser = new ObsGdsParser(tokenStream);
        return parser.Parse();
    }

    public void WriteTo(Stream stream)
    {
        var tokenWriter = new ObsGdsTokenWriter(this);
        ObsGdsWriter.Write(tokenWriter.Tokenize(), stream);
    }

    /// <summary>
    /// Materializes all structures in the GDS file.
    /// </summary>
    public void Materialize()
    {
        var structures = Structures.ToList();
        foreach (var structure in structures)
        {
            structure.Materialize();
        }

        Structures = structures;
    }

    /// <summary>
    ///     Gets the bounding box of a structure.
    ///     Note that currently rotation and other transformations are not taken into account.
    ///     Note that text elements are not taken into account.
    /// </summary>
    /// <param name="structureName">The name of the structure.</param>
    /// <returns>The bounding box of the structure.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when the structure with the given name is not found.</exception>
    public GdsBoundingBox GetBoundingBox(string structureName)
    {
        var structure = GetStructure(structureName);
        if (structure == null)
            throw new KeyNotFoundException($"Structure with name '{structureName}' not found.");

        return structure.GetBoundingBox(GetStructure);
    }
}