using System.Reflection;

namespace GdsSharp.Lib.Test;

public class GdsParserTests
{
    [TestCase("example.cal")]
    [TestCase("inv.gds2")]
    [TestCase("nand2.gds2")]
    [TestCase("xor.gds2")]
    [TestCase("osu018_stdcells.gds2")]
    public void TestParserDoesntCrash(string manifestFile)
    {
        var fileStream =
            Assembly.GetExecutingAssembly().GetManifestResourceStream($"GdsSharp.Lib.Test.Assets.{manifestFile}") ??
            throw new NullReferenceException();
        var tokenizer = new GdsTokenizer(fileStream);
        var tokens = tokenizer.Tokenize();
        var parser = new GdsParser(tokens);
        var file = parser.Parse();
    }
}