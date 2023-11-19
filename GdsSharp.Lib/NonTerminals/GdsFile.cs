namespace GdsSharp.Lib.NonTerminals;

public class GdsFile
{
    public short Version { get; set; } = 5;
    public required string LibraryName { get; set; }
    public required DateTime LastModificationTime { get; set; }
    public required DateTime LastAccessTime { get; set; }
    public List<string> ReferencedLibraries { get; set; } = new();
    public List<string> Fonts { get; set; } = new();
    public string? AttributeDefinitionFile { get; set; }
    public short Generations { get; set; } = 3;
    public required double UserUnits { get; set; }
    public required double PhysicalUnits { get; set; }
    public GdsFormatType FormatType { get; set; } = GdsFormatType.GdsArchive;
    public List<GdsStructure> Structures { get; set; } = new();

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