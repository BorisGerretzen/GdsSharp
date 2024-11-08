using System.Reflection;
using GdsSharp.Lib.Lexing;
using GdsSharp.Lib.Terminals;
using GdsSharp.Lib.Terminals.Abstractions;
using GdsSharp.Lib.Terminals.Records;

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
        GdsWriter.Write(tokenStream, streamOut);

        // remove padding
        var bytesIn = streamIn.ToArray();
        var paddingLength = bytesIn.Reverse().TakeWhile(e => e == 0).Count() - 1;
        bytesIn = bytesIn.SkipLast(paddingLength).ToArray();

        var bytesOut = streamOut.ToArray();

        // Check within 1 because sometimes floating point numbers are slightly different
        Assert.That(bytesOut, Is.EqualTo(bytesIn).Within(1));
    }
}