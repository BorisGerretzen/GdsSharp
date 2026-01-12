using System.Reflection;
using GdsSharp.Lib.Library.VertexStore;
using GdsSharp.Lib.Reading;
using GdsSharp.Lib.Reading.Consumer;
using GdsSharp.Lib.Reading.TokenStream;

namespace GdsSharp.Lib.Test;

public class Testing
{
    [TestCase("example.cal")]
    [TestCase("inv.gds2")]
    [TestCase("nand2.gds2")]
    [TestCase("xor.gds2")]
    [TestCase("gds3d_example.gds")]
    public void Test(string manifestFile)
    {
        for (var i = 0; i < 100; i++) ParseFile(manifestFile);
    }

    private static void ParseFile(string manifestFile)
    {
        using var fileStream =
            Assembly.GetExecutingAssembly().GetManifestResourceStream($"GdsSharp.Lib.Test.Assets.{manifestFile}") ??
            throw new NullReferenceException();
        using var tokenStream = new GdsTokenStream(fileStream);
        var parser = new GdsParser(tokenStream);
        var store = new ChunkedVertexStore();
        var consumer = new GdsLibraryBuilderConsumer(store);
        parser.Parse(consumer);
        var library = consumer.Library;
        Console.WriteLine(library.Info.Name);
    }
}