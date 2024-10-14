using GdsSharp.Lib.Builders;
using GdsSharp.Lib.NonTerminals;
using GdsSharp.Lib.NonTerminals.Elements;

// var file = new GdsFile
// {
//     LibraryName = "Je moeder",
//     PhysicalUnits = 1e-6,
//     UserUnits = 1,
// };
//
// var structure = new GdsStructure
// {
//     Name = "My structure",
// };
//
// var pb = new PathBuilder(1000f);
// for (var i = 0; i < 10_000; i++)
// {
//     pb.Straight(1000, vertices: 10_000);
// }
//
// structure.Elements = pb.Build();
// file.Structures = new[] { structure };
//
// using var fs = new FileStream("example.gds", FileMode.Create, FileAccess.Write);
// file.WriteTo(fs);

using (var fs = new FileStream("example.gds", FileMode.Open, FileAccess.Read))
using (var fs2 = new FileStream("example2.gds", FileMode.Create, FileAccess.Write))
{
    var file = BufferSize(fs);
    file.WriteTo(fs2);
}

return;

GdsFile BufferSize(Stream stream)
{
    var file = GdsFile.From(stream);

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