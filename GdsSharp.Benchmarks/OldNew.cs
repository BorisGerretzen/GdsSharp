using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using GdsSharp.Benchmarks.Obsolete;
using GdsSharp.Benchmarks.Obsolete.Lexing;
using GdsSharp.Lib.Library;
using GdsSharp.Lib.Library.VertexStore;
using GdsSharp.Lib.Reading;
using GdsSharp.Lib.Reading.Consumer;
using GdsSharp.Lib.Reading.TokenStream;

namespace GdsSharp.Benchmarks;

[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.Declared)]
public class OldNew
{
    private const string AssetPath = "Assets/Proprietary/prop.gds";

    [Benchmark(Baseline = true)]
    public GdsFile Old()
    {
        using var fs = File.OpenRead(AssetPath);
        using var tokenStream = new ObsGdsTokenStream(fs);
        var parser = new ObsGdsParser(tokenStream);
        var f = parser.Parse();
        f.Materialize();
        return f;
    }

    [Benchmark]
    public GdsFile NewParserOldStructure()
    {
        using var fs = File.OpenRead(AssetPath);
        var tokenStream = new GdsTokenStream(fs);
        using var parser = new GdsParser(tokenStream);
        var consumer = new OldParserConsumer();
        parser.Parse(consumer);
        return consumer.File;
    }

    [Benchmark]
    public GdsLibrary New()
    {
        using var fs = File.OpenRead(AssetPath);
        var tokenStream = new GdsTokenStream(fs);
        using var parser = new GdsParser(tokenStream);
        var vertexStore = new ChunkedVertexStore();
        var consumer = new GdsLibraryBuilderConsumer(vertexStore);
        parser.Parse(consumer);
        return consumer.Library;
    }

    [Benchmark]
    public GdsLibrary NewDiskBacked()
    {
        using var fs = File.OpenRead(AssetPath);
        var tokenStream = new GdsTokenStream(fs);
        using var parser = new GdsParser(tokenStream);
        var vertexStore = new DiskVertexStore();
        var consumer = new GdsLibraryBuilderConsumer(vertexStore);
        parser.Parse(consumer);
        return consumer.Library;
    }
}