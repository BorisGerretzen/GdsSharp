using BenchmarkDotNet.Attributes;
using GdsSharp.Lib;
using GdsSharp.Lib.Builders;
using GdsSharp.Lib.NonTerminals;
using GdsSharp.Lib.NonTerminals.Elements;

namespace GdsSharp.Benchmarks;

public class BufferedStreamBenchmark
{

    [GlobalSetup]
    public void Setup()
    {
        var file = new GdsFile
        {
            LibraryName = "Test",
            PhysicalUnits = 1e-6,
            UserUnits = 1,
        };

        var structure = new GdsStructure
        {
            Name = "My structure",
        };

        var pb = new PathBuilder(1000f);
        for (var i = 0; i < 1000; i++)
        {
            pb.Straight(1000, vertices: 10000);
        }

        structure.Elements = pb.Build();
        file.Structures = new[] { structure };

        using var fs = new FileStream("example.gds", FileMode.Create, FileAccess.Write);
        file.WriteTo(fs);
    }
    
    [GlobalCleanup]
    public void Cleanup()
    {
        File.Delete("example.gds");
    }
    
    private Stream _stream = null!;
    
    [IterationSetup]
    public void IterationSetup()
    {
        _stream = new FileStream("example.gds", FileMode.Open, FileAccess.Read);
    }
    
    [IterationCleanup]
    public void IterationCleanup()
    {
        _stream.Dispose();
    }
    
    [Benchmark]
    public GdsFile NoBuffer()
    {
        var file = GdsFile.From(_stream);

        file.Structures = file.Structures.ToList();
        foreach(var structure in file.Structures)
        {
            structure.Elements = structure.Elements.ToList();
            foreach (var element in structure.Elements)
            {
                if (element is not {Element: GdsBoundaryElement b}) continue;
                
                b.Points = b.Points.ToList();
            }
        }
        
        return file;
    }
    
        
    [Benchmark]
    [Arguments(4*1024)]
    [Arguments(32*1024)]
    public GdsFile Buffer(int size)
    {
        var bufferedStream = new BufferedStream(_stream, size);
        
        var file = GdsFile.From(bufferedStream);

        file.Structures = file.Structures.ToList();
        foreach(var structure in file.Structures)
        {
            structure.Elements = structure.Elements.ToList();
            foreach (var element in structure.Elements)
            {
                if (element is not {Element: GdsBoundaryElement b}) continue;
                
                b.Points = b.Points.ToList();
            }
        }
        
        return file;
    }
}