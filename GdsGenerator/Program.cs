using GdsSharp.Lib;
using RBush;

await using var file = File.OpenRead("example.gds");
var gds = GdsFile.From(file);
Console.WriteLine("read GDS file");

var tree = gds.CreateSpatialIndex();

Console.WriteLine("created spatial index");

var structures = tree.Search(new Envelope(-1260, 4470179, 11113, 4471014));
foreach (var structure in structures)
{
    Console.WriteLine(structure.StructureName);
}