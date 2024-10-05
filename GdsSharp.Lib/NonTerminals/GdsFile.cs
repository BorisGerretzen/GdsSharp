namespace GdsSharp.Lib.NonTerminals;

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
        var tokenizer = new GdsTokenizer(stream);
        var tokens = tokenizer.Tokenize();
        var parser = new GdsParser(tokens);
        return parser.Parse();
    }

    public void WriteTo(Stream stream)
    {
        var tokenWriter = new GdsTokenWriter(this);
        var tokens = tokenWriter.Tokenize();
        GdsWriter.Write(tokens, stream);
    }
}