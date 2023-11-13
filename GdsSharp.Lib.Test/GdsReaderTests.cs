using System.Reflection;

namespace GdsSharp.Lib.Test;

public class GdsReaderTests
{
    private Stream _stream1 = null!;
    private Stream _stream2 = null!;

    [SetUp]
    public void SetUp()
    {
        _stream1 = Assembly.GetExecutingAssembly().GetManifestResourceStream("GdsSharp.Lib.Test.Assets.example.cal") ?? throw new NullReferenceException();
        _stream2 = Assembly.GetExecutingAssembly().GetManifestResourceStream("GdsSharp.Lib.Test.Assets.inv.gds2") ?? throw new NullReferenceException();
    }
    
    [Test]
    public void TokenizeDoesntCrashOnExample1()
    {
        var parser = new GdsReader(_stream1);
        var tokens = parser.Tokenize().ToList();
        Assert.That(tokens, Has.Count.EqualTo(76));
    }
    
    [Test]
    public void TokenizeDoesntCrashOnExample2()
    {
        var parser = new GdsReader(_stream2);
        var tokens = parser.Tokenize().ToList();
        Assert.That(tokens, Has.Count.EqualTo(425));
    }
}