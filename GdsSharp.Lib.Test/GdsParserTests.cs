using System.Reflection;
using GdsSharp.Lib.Lexing;
using GdsSharp.Lib.NonTerminals.Elements;

namespace GdsSharp.Lib.Test;

public class GdsParserTests
{
    [TestCase("example.cal")]
    [TestCase("inv.gds2")]
    [TestCase("nand2.gds2")]
    [TestCase("xor.gds2")]
    [TestCase("gds3d_example.gds")]
    public void TestParserDoesntCrash(string manifestFile)
    {
        using var fileStream =
            Assembly.GetExecutingAssembly().GetManifestResourceStream($"GdsSharp.Lib.Test.Assets.{manifestFile}") ??
            throw new NullReferenceException();
        using var stream = new GdsTokenStream(fileStream);
        var parser = new GdsParser(stream);
        var file = parser.Parse();
        file.Materialize();
    }
}