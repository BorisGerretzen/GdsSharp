using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using GdsSharp.Benchmarks.Obsolete;
using GdsSharp.Lib;

namespace GdsSharp.Benchmarks;

[SimpleJob(RuntimeMoniker.Net80)]
[MemoryDiagnoser]
public class GdsDoubleBenchmark
{
    private byte[] _buffer;
    private byte[][] _dataInputs;
    private double[] _values;

    [Params(10_000)] public int N;

    [GlobalSetup]
    public void Setup()
    {
        _values = new double[N];
        _dataInputs = new byte[N][];
        _buffer = new byte[8];

        var random = new Random(42);

        for (var i = 0; i < N; i++)
        {
            var exponent = random.NextDouble() * 150.0 - 75.0;
            var mantissa = random.NextDouble();
            var val = mantissa * Math.Pow(10, exponent);
            _values[i] = random.Next(2) == 0 ? val : -val;

            // Create pre-filled byte arrays for the Read test
            _dataInputs[i] = new byte[8];
            GdsDoubleConverter.ToGdsBytes(_values[i], _dataInputs[i]);
        }
    }

    [Benchmark(Baseline = true)]
    public void Write_Legacy()
    {
        for (var i = 0; i < N; i++)
        {
            var old = new GdsDouble(_values[i]);
            old.WriteTo(_buffer);
        }
    }

    [Benchmark]
    public void Write_Optimized()
    {
        for (var i = 0; i < N; i++) GdsDoubleConverter.ToGdsBytes(_values[i], _buffer);
    }

    [Benchmark]
    public void Read_Legacy()
    {
        double sum = 0;
        for (var i = 0; i < N; i++)
        {
            var old = new GdsDouble(_dataInputs[i]);
            sum += old.AsDouble();
        }
    }

    [Benchmark]
    public void Read_Optimized()
    {
        double sum = 0;
        for (var i = 0; i < N; i++) sum += GdsDoubleConverter.FromGdsBytes(_dataInputs[i]);
    }
}