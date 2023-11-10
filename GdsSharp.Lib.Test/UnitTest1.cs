using System.Reflection;

namespace GdsSharp.Lib.Test;

public class Tests
{
    private Stream _stream;

    [SetUp]
    public void SetUp()
    {
        _stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("GdsSharp.Lib.Test.Assets.example.cal") ?? throw new NullReferenceException();
    }
    
    [Test]
    public void Test1()
    {
        var parser = new GdsReader(_stream);
        parser.Parse();
    }
}