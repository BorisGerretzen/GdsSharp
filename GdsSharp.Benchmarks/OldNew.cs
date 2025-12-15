using BenchmarkDotNet.Attributes;
using GdsSharp.Lib;
using GdsSharp.Lib.Lexing;
using GdsSharp.Lib.Old;
using GdsSharp.Lib.Old.Lexing;
using GdsSharp.Lib.Parsing;
using GdsSharp.Lib.Parsing.Consumer;

namespace GdsSharp.Benchmarks;

[MemoryDiagnoser]
public class OldNew
{
    private Stream? _stream;
    
    [IterationSetup]
    public void Setup()
    {
        _stream = File.OpenRead("Assets/example.gds");
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
}