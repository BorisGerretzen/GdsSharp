using System.Reflection;
using GdsSharp.Lib.Library.VertexStore;
using GdsSharp.Lib.Obsolete;
using GdsSharp.Lib.Reading;
using GdsSharp.Lib.Reading.Consumer;
using GdsSharp.Lib.Reading.TokenStream;
using GdsSharp.Lib.Writing;

namespace GdsSharp.Lib.Test;

public class GdsWriterTests
{
    [TestCase("example.cal")]
    [TestCase("inv.gds2")]
    [TestCase("nand2.gds2")]
    [TestCase("xor.gds2")]
    [TestCase("gds3d_example.gds")]
    public void TestWriterWritesIdentical(string manifestFile)
    {
        using var streamIn = new MemoryStream();
        using var streamOut = new MemoryStream();

        using var fileStream =
            Assembly.GetExecutingAssembly().GetManifestResourceStream($"GdsSharp.Lib.Test.Assets.{manifestFile}") ??
            throw new NullReferenceException();
        fileStream.CopyTo(streamIn);
        fileStream.Position = 0;

        using var tokenStream = new GdsTokenStream(fileStream);
        var vertexStore = new MemoryVertexStore();
        var consumer = new GdsLibraryBuilderConsumer(vertexStore);
        var parser = new GdsParser(tokenStream);
        parser.Parse(consumer);

        var writer = new GdsWriter(streamOut);
        writer.Write(consumer.Library, vertexStore);

        // remove padding
        var bytesIn = streamIn.ToArray();
        var paddingLength = bytesIn.Reverse().TakeWhile(e => e == 0).Count() - 1;
        bytesIn = bytesIn.SkipLast(paddingLength).ToArray();

        var bytesOut = streamOut.ToArray();
        
        // write to disk for manual inspection
        File.WriteAllBytes($"new_{manifestFile}", bytesOut);

        // Check within 1 because sometimes floating point numbers are slightly different
        Assert.That(bytesOut, Has.Length.EqualTo(bytesIn.Length));
        Assert.That(bytesOut, Is.EqualTo(bytesIn).Within(1));
    }
}