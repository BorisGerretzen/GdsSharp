using BenchmarkDotNet.Attributes;
using GdsSharp.Lib.Library;
using GdsSharp.Lib.Library.VertexStore;
using GdsSharp.Lib.Obsolete;
using GdsSharp.Lib.Obsolete.Lexing;
using GdsSharp.Lib.Reading;
using GdsSharp.Lib.Reading.Consumer;
using GdsSharp.Lib.Reading.TokenStream;

namespace GdsSharp.Benchmarks;

[MemoryDiagnoser]
public class OldNew
{
    private Stream? _stream;

    [IterationSetup]
    public void Setup()
    {
        _stream = File.OpenRead("Assets/output.gds");
    }

    [IterationCleanup]
    public void Cleanup()
    {
        _stream?.Dispose();
        _stream = null;
    }

    [Benchmark]
    public GdsFile Old()
    {
        using var tokenStream = new GdsTokenStream(_stream!);
        var parser = new GdsParser(tokenStream);
        var f = parser.Parse();
        f.Materialize();
        return f;
    }

    [Benchmark]
    public GdsFile New()
    {
        using var tokenStream = new NewGdsTokenStream(_stream!);
        var parser = new NewGdsParser(tokenStream);
        var consumer = new OldParserConsumer();
        parser.Parse(consumer);
        return consumer.File;
    }

    [Benchmark]
    public GdsLibrary Newest()
    {
        using var tokenStream = new NewGdsTokenStream(_stream!);
        var parser = new NewGdsParser(tokenStream);
        var vertexStore = new MemoryVertexStore();
        var consumer = new GdsLibraryBuilderConsumer(vertexStore);
        parser.Parse(consumer);
        return consumer.Library;
    }
}