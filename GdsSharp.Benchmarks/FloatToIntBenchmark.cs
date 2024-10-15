using BenchmarkDotNet.Attributes;

namespace GdsSharp.Benchmarks;

public class FloatToIntBenchmark
{
    private readonly float[] _xs = new float[1000];
    private readonly float[] _ys = new float[1000];
    
    [GlobalSetup]
    public void Setup()
    { 
        var random = new Random();
        for (int i = 0; i < _xs.Length; i++)
        {
            _xs[i] = (float)random.NextDouble() * 1000;
            _ys[i] = (float)random.NextDouble() * 1000;
        }
    }
    
    [Benchmark]
    public IntPoint[] FloatToIntRound()
    {
        var points = new IntPoint[_xs.Length];
        for (int i = 0; i < _xs.Length; i++)
        {
            points[i] = new IntPoint
            {
                X = (int)MathF.Round(_xs[i]),
                Y = (int)MathF.Round(_ys[i])
            };
        }

        return points;
    }
    
    [Benchmark]
    public IntPoint[] FloatToIntAdd()
    {
        var points = new IntPoint[_xs.Length];
        for (int i = 0; i < _xs.Length; i++)
        {
            points[i] = new IntPoint
            {
                X = (int)(_xs[i] + 0.5f),
                Y = (int)(_ys[i] + 0.5f)
            };
        }

        return points;
    }

    
    public struct IntPoint
    {
        public int X;
        public int Y;
    }
}