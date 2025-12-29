using GdsSharp.Lib.InternalDb.VertexStore;
using GdsSharp.Lib.Reading;
using GdsSharp.Lib.Reading.Consumer;
using GdsSharp.Lib.Reading.TokenStream;
using GdsSharp.Lib.Writing;

var runtimes = new List<TimeSpan>();
for (var i = 0; i < 30; i++)
{
    await using var fs = new FileStream("flexit_lasers_publ.gds", FileMode.Open, FileAccess.Read, FileShare.Read);
    var ts = new NewGdsTokenStream(fs);
    var vs = new MemoryVertexStore(20_000_000);
    var consumer = new InternalDbConsumer(vs);
    var parser = new NewGdsParser(ts);
    parser.Parse(consumer);
    var lib = consumer.Library;
    await using var fsOut = new FileStream("outt.gds", FileMode.Create, FileAccess.Write, FileShare.None);
    // using var fsOut = new MemoryStream();
    var writer = new NewGdsWriter(fsOut);
    var tBefore = DateTime.UtcNow;
    writer.Write(lib, vs);
    var tAfter = DateTime.UtcNow;
    runtimes.Add(tAfter - tBefore);
    Console.WriteLine($"Run {i + 1}: {(tAfter - tBefore).TotalMilliseconds}ms");
}

var avgMs = runtimes.Average(ts => ts.TotalMilliseconds);
Console.WriteLine($"Average time: {avgMs}ms");


// var runtimes = new List<TimeSpan>();
// for (var i = 0; i < 30; i++)
// {
//     await using var fs = new FileStream("flexit_lasers_publ.gds", FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.SequentialScan);
//     var tBefore = DateTime.UtcNow;
//     var ts = new NewGdsTokenStream(fs);
//     var vs = new MemoryVertexStore(20_000_000);
//     var consumer = new InternalDbConsumer(vs);
//     var parser = new NewGdsParser(ts);
//     parser.Parse(consumer);
//     var lib = consumer.Library;
//     var tAfter = DateTime.UtcNow;
//     runtimes.Add(tAfter - tBefore);
//     Console.WriteLine($"Run {i + 1}: {(tAfter - tBefore).TotalMilliseconds}ms");
// }
// var avgMs = runtimes.Average(ts => ts.TotalMilliseconds);
// Console.WriteLine($"Average time: {avgMs}ms");