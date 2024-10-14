using System.Reflection;
using FluentAssertions;

namespace GdsSharp.Lib.Test;

public class IntegrationTests
{
    [TestCase("example.cal")]
    [TestCase("inv.gds2")]
    [TestCase("nand2.gds2")]
    [TestCase("xor.gds2")]
    // [TestCase("osu018_stdcells.gds2")]
    public void TestRoundTrip(string manifestFile)
    {
        var fileStream =
            Assembly.GetExecutingAssembly().GetManifestResourceStream($"GdsSharp.Lib.Test.Assets.{manifestFile}") ??
            throw new NullReferenceException();
        var tokenStream = new GdsTokenStream(fileStream);
        var parser = new GdsParser(tokenStream);
        var file = parser.Parse();
        file.Structures = file.Structures.ToList();
        
        using var fs2 = new FileStream("jemoeder.gds", FileMode.Create);
        file.WriteTo(fs2);
        
        
        // file.Should().BeEquivalentTo(fileNew);
    }
}