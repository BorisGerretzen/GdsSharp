using GdsSharp.Lib.Lexing;
using GdsSharp.Lib.NonTerminals;
using GdsSharp.Lib.NonTerminals.Enum;

namespace GdsSharp.Lib;

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

    public static GdsFile From(Stream stream)
    {
        using var tokenStream = new GdsTokenStream(stream);
        var parser = new GdsParser(tokenStream);
        return parser.Parse();
    }

    public void WriteTo(Stream stream)
    {
        var tokenWriter = new GdsTokenWriter(this);
        GdsWriter.Write(tokenWriter.Tokenize(), stream);
    }
}