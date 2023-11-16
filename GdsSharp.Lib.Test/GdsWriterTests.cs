using System.Reflection;
using GdsSharp.Lib.Parsing;
using GdsSharp.Lib.Parsing.Tokens;

namespace GdsSharp.Lib.Test;

public class GdsWriterTests
{
    [Test]
    public void TestWriterWritesHeaders()
    {
        var records = new List<IGdsWriteableRecord>
        {
            new GdsRecordAngle
            {
                Value = 1.2345d
            }
        };

        var stream = new MemoryStream();
        GdsWriter.Write(records, stream);
        var bytes = stream.ToArray();

        Assert.That(bytes, Has.Length.EqualTo(records.First().GetLength() + GdsHeader.RecordSize));
    }

    [TestCase("example.cal")]
    [TestCase("inv.gds2")]
    [TestCase("nand2.gds2")]
    [TestCase("xor.gds2")]
    [TestCase("osu018_stdcells.gds2")]
    public void TestWriterWritesIdentical(string manifestFile)
    {
        var streamIn = new MemoryStream();
        var streamOut = new MemoryStream();

        var fileStream =
            Assembly.GetExecutingAssembly().GetManifestResourceStream($"GdsSharp.Lib.Test.Assets.{manifestFile}") ??
            throw new NullReferenceException();
        fileStream.CopyTo(streamIn);
        fileStream.Position = 0;

        var parser = new GdsReader(fileStream);
        var tokens = parser.Tokenize().ToList();
        GdsWriter.Write(tokens, streamOut);

        // remove padding
        var bytesIn = streamIn.ToArray();
        var paddingLength = bytesIn.Reverse().TakeWhile(e => e == 0).Count() - 1;
        bytesIn = bytesIn.SkipLast(paddingLength).ToArray();

        var bytesOut = streamOut.ToArray();

        // Check within 1 because sometimes floating point numbers are slightly different
        Assert.That(bytesOut, Is.EqualTo(bytesIn).Within(1));
    }
}