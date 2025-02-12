using System.Linq.Expressions;
using System.Reflection;
using GdsSharp.Lib;
using GdsSharp.Lib.Abstractions;
using GdsSharp.Lib.Binary;
using GdsSharp.Lib.New;
using GdsSharp.Lib.Terminals;
using GdsSharp.Lib.Terminals.Abstractions;
using GdsSharp.Lib.Terminals.Records;

// await using var file = File.OpenRead("example.gds");

// Build activators
Dictionary<ushort, Func<IGdsRecord>> Activators = new();
var assembly = Assembly.GetAssembly(typeof(GdsStreamOperator));
if (assembly is null) throw new InvalidOperationException("Could not get assembly");
var recordTypes = assembly.GetTypes()
    .Where(t => t is { IsClass: true, IsAbstract: false } && typeof(IGdsRecord).IsAssignableFrom(t));
foreach (var recordType in recordTypes)
{
    var activator = Expression.Lambda<Func<IGdsRecord>>(Expression.New(recordType)).Compile();
    var record = activator.Invoke();
    if (record is null) throw new InvalidOperationException($"Could not get activator for {recordType.Name}");
    Activators.Add(record.Code, activator);
}
foreach (var value in Enum.GetValues<GdsRecordNoDataType>())
    Activators.Add((ushort)value, () => new GdsRecordNoData { Type = value });

using (var fsGen = new FileStream("data.xy", FileMode.Create))
{
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

// read from file
using var fs = File.OpenRead("data.xy");
using var reader = new GdsBinaryReader(fs);
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

return;



// var binaryReader = new GdsBinaryReader(file);
// var reader = new NewGdsReader(binaryReader);
// var header = reader.ReadHeader();
//
// Console.WriteLine($"{header.RecordType:X} {header.Length}");


// // using var tokenStream = new GdsTokenStream(file);
//
// var gds = GdsFile.From(file);
// var startTime = DateTime.Now; 
// gds.Materialize();
// // var index = GdsIndex.Create(tokenStream);
//
// Console.WriteLine($"time: {(DateTime.Now - startTime).TotalSeconds:F2} seconds");
//
//
// // var tree = gds.CreateSpatialIndex();
// //
// // Console.WriteLine("created spatial index");
// //
// // var structures = tree.Search(new Envelope(-1260, 4470179, 11113, 4471014));
// // foreach (var structure in structures)
// // {
// //     Console.WriteLine(structure.StructureName);
// // }