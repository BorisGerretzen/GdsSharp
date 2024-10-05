using BenchmarkDotNet.Attributes;
using GdsSharp.Benchmarks.PathBuilder.Implementations;
using GdsSharp.Lib.NonTerminals.Elements;

namespace GdsSharp.Benchmarks.PathBuilder;

[MemoryDiagnoser]
public class PathBuilderBenchmark
{
    private PathBuilderListReverse _listReverse = null!;
    private PathBuilderArray _array = null!;
    private PathBuilderListReversePrealloc _listReversePrealloc = null!;
    private PathBuilderListReverseSpan _listReverseSpan = null!;
    private PathBuilderListReverseFactorExtract _factorExtract = null!;
    
    private PathBuilderStack _stack = null!;
    private PathBuilderBase _base = null!;
    private PathBuilderSpan _span = null!;
    
    [GlobalSetup]
    public void GlobalSetup()
    {
        _listReverse = new(1.0f);
        _array = new(1.0f);
        _listReversePrealloc = new(1.0f);
        _listReverseSpan = new(1.0f);
        _factorExtract = new(1.0f);
        
        _stack = new(1.0f);
        _base = new(1.0f);
        _span = new(1.0f);

        List<IPathBuilder> builders =
        [
            _listReverse,
            _array,
            _listReversePrealloc,
            _listReverseSpan,
            _factorExtract,
            _stack,
            _base,
            _span
        ];

        foreach (var builder in builders)
        {
            builder
                .Straight(2000)
                .BendDeg(-45, 500)
                .Straight(100, widthEnd: 250)
                .Straight(100)
                .Straight(100, widthEnd: 100)
                .BendDeg(-45, 500)
                .Straight(100)
                .Straight(200, 250)
                .BendDeg(180, 300, vertices: 10000)
                .BendDeg(-180, 300)
                .BendDeg(-180, 900, f => MathF.Cos(f * 50) * 100 + 150, vertices: 10000)
                .Bezier(b => b
                        .AddPoint(0, 0)
                        .AddPoint(0, 1000)
                        .AddPoint(2000, 1000)
                        .AddPoint(1000, 0),
                    t => 250 - (250 - 50) * t, vertices: 10000)
                .Straight(800);
        }
    }
    
    [Benchmark] public List<GdsElement> PathBuilderArray() => _array.Build().ToList();
    
    [Benchmark] public List<GdsElement> PathBuilderFactorExtract() => _factorExtract.Build().ToList();
    
    [Benchmark] public List<GdsElement> PathBuilderListReverse() => _listReverse.Build().ToList();
    
    [Benchmark] public List<GdsElement> PathBuilderListReversePrealloc() => _listReversePrealloc.Build().ToList();
    
    [Benchmark] public List<GdsElement> PathBuilderListReverseSpan() => _listReverseSpan.Build().ToList();
    
    [Benchmark] public List<GdsElement> PathBuilderStack() => _stack.Build().ToList();
    
    [Benchmark] public List<GdsElement> PathBuilderBase() => _base.Build().ToList();
    
    // [Benchmark]
    // public List<GdsElement> PathBuilderSpan() => _pathBuilderSpan.Build().ToList();
}