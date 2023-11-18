using System.Reflection;
using FluentAssertions;

namespace GdsSharp.Lib.Test;

public class IntegrationTests
{
    [TestCase("example.cal")]
    [TestCase("inv.gds2")]
    [TestCase("nand2.gds2")]
    [TestCase("xor.gds2")]
    [TestCase("osu018_stdcells.gds2")]
    public void TestRoundTrip(string manifestFile)
    {
        var fileStream =
            Assembly.GetExecutingAssembly().GetManifestResourceStream($"GdsSharp.Lib.Test.Assets.{manifestFile}") ??
            throw new NullReferenceException();
        
        var tokenizer = new GdsTokenizer(fileStream);
        var tokens = tokenizer.Tokenize().ToList();
        
        var parser = new GdsParser(tokens);
        var file = parser.Parse();

        var tokenWriter = new GdsTokenWriter(file);
        var tokensNew = tokenWriter.Tokenize().ToList();
        
        var parserNew = new GdsParser(tokensNew);
        var fileNew = parserNew.Parse();

        file.Should().BeEquivalentTo(fileNew);
    }
}