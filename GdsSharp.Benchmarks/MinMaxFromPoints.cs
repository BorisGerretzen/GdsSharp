using System.Numerics;
using System.Runtime.InteropServices;
using BenchmarkDotNet.Attributes;
using GdsSharp.Lib;
using GdsSharp.Lib.Library.BoundingBox;

namespace GdsSharp.Benchmarks;

public class MinMaxFromPoints
{
    [Params(10, 100, 1_000, 10_000)]
    public int N;

    private GdsPoint[] _points;

    [GlobalSetup]
    public void GlobalSetup()
    {
        _points = new GdsPoint[N];
        
        var random = new Random(42); 
        
        for (int i = 0; i < _points.Length; i++)
        {
            _points[i] = new GdsPoint(
                random.Next(-10000, 10000),
                random.Next(-10000, 10000)
            );
        }
    }

    [Benchmark(Baseline = true)]
    public GdsBoundingBox ForeachMinMax()
    {
        var minX = int.MaxValue;
        var minY = int.MaxValue;
        var maxX = int.MinValue;
        var maxY = int.MinValue;

        foreach (var point in _points)
        {
            if (point.X < minX) minX = point.X;
            if (point.Y < minY) minY = point.Y;
            if (point.X > maxX) maxX = point.X;
            if (point.Y > maxY) maxY = point.Y;
        }

        return new GdsBoundingBox(new GdsPoint(minX, minY), new GdsPoint(maxX, maxY));
    }

    [Benchmark]
    public GdsBoundingBox Simd()
    {
        if(!Vector.IsHardwareAccelerated) throw new InvalidOperationException("SIMD not supported on this hardware.");
        
        // Memory layout: [X1, Y1, X2, Y2, X3, Y3...]
        var rawValues = MemoryMarshal.Cast<GdsPoint, int>(_points);

        var minX = int.MaxValue;
        var minY = int.MaxValue;
        var maxX = int.MinValue;
        var maxY = int.MinValue;

        var i = 0;

        if (Vector.IsHardwareAccelerated && rawValues.Length >= Vector<int>.Count)
        {
            var vMin = new Vector<int>(int.MaxValue);
            var vMax = new Vector<int>(int.MinValue);

            // Loop until we can't fill a full vector
            for (; i <= rawValues.Length - Vector<int>.Count; i += Vector<int>.Count)
            {
                var v = new Vector<int>(rawValues[i..]);
                vMin = Vector.Min(vMin, v);
                vMax = Vector.Max(vMax, v);
            }

            // vMin now contains [minX_sub1, minY_sub1, minX_sub2, minY_sub2...]
            for (var j = 0; j < Vector<int>.Count; j += 2)
            {
                minX = Math.Min(minX, vMin[j]);
                minY = Math.Min(minY, vMin[j + 1]);

                maxX = Math.Max(maxX, vMax[j]);
                maxY = Math.Max(maxY, vMax[j + 1]);
            }
        }

        // Process any remaining integers that didn't fit in the vector
        for (; i < rawValues.Length; i += 2)
        {
            var x = rawValues[i];
            var y = rawValues[i + 1];

            if (x < minX) minX = x;
            if (x > maxX) maxX = x;

            if (y < minY) minY = y;
            if (y > maxY) maxY = y;
        }

        return new GdsBoundingBox(new GdsPoint(minX, minY), new GdsPoint(maxX, maxY));
    }
}