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

    [Test]
    public void TestWriterWritesIdentical()
    {
        var streamIn = new MemoryStream();
        var streamOut = new MemoryStream();

        var fileStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("GdsSharp.Lib.Test.Assets.example.cal") ?? throw new NullReferenceException();
        fileStream.CopyTo(streamIn);
        fileStream.Position = 0;

        var parser = new GdsReader(fileStream);
        var tokens = parser.Tokenize().ToList();
        GdsWriter.Write(tokens, streamOut);

        var bytesIn = streamIn.ToArray();
        var bytesOut = streamOut.ToArray();
        // using var fStream = File.OpenWrite("output.gds2");
        // streamOut.WriteTo(fStream);

        Assert.That(bytesOut, Is.EqualTo(bytesIn));
    }
}