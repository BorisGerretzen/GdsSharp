using System.Reflection;
using GdsSharp.Lib.Library.VertexStore;
using GdsSharp.Lib.Reading;
using GdsSharp.Lib.Reading.Consumer;
using GdsSharp.Lib.Reading.TokenStream;

namespace GdsSharp.Lib.Test.Reading;

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

        var vertexStore = new ChunkedVertexStore();
        var consumer = new GdsLibraryBuilderConsumer(vertexStore);
        var parser = new GdsParser(stream);
        parser.Parse(consumer);
    }
}