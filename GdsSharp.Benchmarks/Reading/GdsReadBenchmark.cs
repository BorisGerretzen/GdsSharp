using System.Linq.Expressions;
using System.Reflection;
using BenchmarkDotNet.Attributes;
using GdsSharp.Lib;
using GdsSharp.Lib.Abstractions;
using GdsSharp.Lib.Binary;
using GdsSharp.Lib.New;
using GdsSharp.Lib.Terminals;
using GdsSharp.Lib.Terminals.Abstractions;
using GdsSharp.Lib.Terminals.Records;

namespace GdsSharp.Benchmarks.Reading;

[MemoryDiagnoser]
public class GdsReadBenchmark
{
    private FileStream _fs = null!;
    
    [Params(1000)]
    public int Iterations { get; set; }
    
    [GlobalSetup]
    public void Setup()
    {
        using var fsGen = new FileStream("data.xy", FileMode.Create);
        using var writer = new GdsBinaryWriter(fsGen);
        var genHeader = new GdsHeader
        {
            Code = 0x1003,
            Length = ushort.MaxValue
        };
        ((IGdsSimpleWrite)genHeader).Write(writer);
        const int numCoordinates = (ushort.MaxValue - 4) / (2 * sizeof(int));
        for (var i = 0; i < numCoordinates; i++)
        {
            writer.Write(i);
            writer.Write(i);
        }
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        File.Delete("data.xy");
    }

    [IterationSetup]
    public void IterationSetup()
    {
        _fs = File.OpenRead("data.xy");
    }
    
    [IterationCleanup]
    public void IterationCleanup()
    {
        _fs.Dispose();
    }
    
    [Benchmark]
    public GdsRecordXy[] Old()
    {
        using var reader = new GdsBinaryReader(_fs);
        var arr = new GdsRecordXy[Iterations];
        for (var ctr = 0; ctr < Iterations; ctr++)
        {
            _fs.Position = 0;
            var header = new GdsHeader();
            ((IGdsSimpleRead)header).Read(reader, header);

            var coordinates = new List<GdsPoint>();
            for (var i = 0; i < header.NumToRead / 8; i++)
            {
                coordinates.Add(new GdsPoint(reader.ReadInt32(), reader.ReadInt32()));
            }

            var xyRecord = new GdsRecordXy
            {
                NumPoints = header.NumToRead / 8,
                Coordinates = coordinates
            };
            arr[ctr] = xyRecord;
        }
        
        return arr;
    }

    [Benchmark]
    public NewGdsRecordXy[] New()
    {
        var reader = new NewGdsReader(_fs);
        var arr = new NewGdsRecordXy[Iterations];
        for (var ctr = 0; ctr < Iterations; ctr++)
        {
            _fs.Position = 0;
            var header = reader.ReadHeader();
            var record = reader.ReadRecordXy((header.Length - 4) / 8);
            arr[ctr] = record;
        }
        
        return arr;
    }
}