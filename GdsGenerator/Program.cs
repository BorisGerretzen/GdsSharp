using GdsSharp.Lib.InternalDb.VertexStore;
using GdsSharp.Lib.Lexing;
using GdsSharp.Lib.Parsing;
using GdsSharp.Lib.Parsing.Consumer;
using GdsSharp.Lib.Writing;

await using var fs = new FileStream("in.gds", FileMode.Open, FileAccess.Read, FileShare.Read);
var tBefore = DateTime.UtcNow;
var ts = new NewGdsTokenStream(fs);
var vs = new MemoryVertexStore();
var consumer = new InternalDbConsumer(vs);
var parser = new NewGdsParser(ts);
parser.Parse(consumer);
var lib = consumer.Library;

await using var fsOut = new FileStream("outt.gds", FileMode.Create, FileAccess.Write, FileShare.None);
var writer = new NewGdsWriter(fsOut);
writer.Write(lib, vs);