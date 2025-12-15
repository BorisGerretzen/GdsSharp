using System.Reflection;
using GdsSharp.Lib.InternalDb.VertexStore;
using GdsSharp.Lib.Lexing;
using GdsSharp.Lib.Parsing;
using GdsSharp.Lib.Parsing.Consumer;

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
        using var fileStream =
            Assembly.GetExecutingAssembly().GetManifestResourceStream($"GdsSharp.Lib.Test.Assets.{manifestFile}") ??
            throw new NullReferenceException();
        using var tokenStream = new NewGdsTokenStream(fileStream);
        var parser = new NewGdsParser(tokenStream);
        var store = new MemoryVertexStore();
        var consumer = new InternalDbConsumer(store);
        parser.Parse(consumer);
        var library = consumer.Library;
        Console.WriteLine(library.Info.Name);
    }
}