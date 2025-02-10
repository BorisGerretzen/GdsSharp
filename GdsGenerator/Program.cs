using GdsSharp.Lib;
using GdsSharp.Lib.Index;
using GdsSharp.Lib.Lexing;

await using var file = File.OpenRead("example.gds");
// using var tokenStream = new GdsTokenStream(file);

var gds = GdsFile.From(file);
var startTime = DateTime.Now; 
gds.Materialize();
// var index = GdsIndex.Create(tokenStream);

Console.WriteLine($"time: {(DateTime.Now - startTime).TotalSeconds:F2} seconds");


// var tree = gds.CreateSpatialIndex();
//
// Console.WriteLine("created spatial index");
//
// var structures = tree.Search(new Envelope(-1260, 4470179, 11113, 4471014));
// foreach (var structure in structures)
// {
//     Console.WriteLine(structure.StructureName);
// }