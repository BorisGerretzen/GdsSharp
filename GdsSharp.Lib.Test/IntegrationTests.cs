using System.Reflection;
using FluentAssertions;
using GdsSharp.Lib.Lexing;
using GdsSharp.Lib.NonTerminals.Elements;

namespace GdsSharp.Lib.Test;

public class IntegrationTests
{
    [TestCase("example.cal")]
    [TestCase("inv.gds2")]
    [TestCase("nand2.gds2")]
    [TestCase("xor.gds2")]
    [TestCase("gds3d_example.gds")]
    public void TestRoundTrip(string manifestFile)
    {
        var fileStream =
            Assembly.GetExecutingAssembly().GetManifestResourceStream($"GdsSharp.Lib.Test.Assets.{manifestFile}") ??
            throw new NullReferenceException();
        using var tokenStream = new GdsTokenStream(fileStream);
        var parser = new GdsParser(tokenStream);
        var file = parser.Parse();
        
        var ms = new MemoryStream();
        file.WriteTo(ms);
        ms.Position = 0;
        
        using var tokenStreamNew = new GdsTokenStream(ms);
        var parserNew = new GdsParser(tokenStreamNew);
        var fileNew = parserNew.Parse();
        
        file.Materialize();
        fileNew.Materialize();
        
        file.Should().BeEquivalentTo(fileNew);
    }
}