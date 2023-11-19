using System.CommandLine;
using System.Data;
using System.Text;
using System.Text.Json;
using GdsSharp.Lib;
using GdsSharp.Lib.NonTerminals;
using GdsSharp.Lib.NonTerminals.Elements;

// Root
var numArgument = new Argument<int>(
    name: "num",
    description: "The number of objects to generate.");
var rootCommand = new RootCommand("Sample app for System.CommandLine");

// file
var subFile = new Command("file", "Write to a file instead of stdout");
var fileArgument = new Argument<string>(
    name: "file",
    description: "The file to write to.");
subFile.AddArgument(numArgument);
subFile.AddArgument(fileArgument);
subFile.SetHandler((num, file) =>
    {
        var gds = GenerateFile(num);
        var tokenWriter = new GdsTokenWriter(gds);
        using (var write = File.OpenWrite("myOut.json"))
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
            };
            var json = JsonSerializer.Serialize((object[])tokenWriter.Tokenize().ToArray(), options);
            var bytes = Encoding.UTF8.GetBytes(json);
            write.Write(bytes);
            write.Flush();
        }

        using (var write = File.OpenWrite(file))
        {
            gds.WriteTo(write);
        }
    },
    numArgument, fileArgument);
rootCommand.AddCommand(subFile);

return await rootCommand.InvokeAsync(args);

static GdsFile GenerateFile(int num)
{
    var file = new GdsFile
    {
        LibraryName = "RandomObjects",
        LastModificationTime = DateTime.Now,
        FormatType = GdsFormatType.GdsArchive,
        LastAccessTime = DateTime.Now,
        UserUnits = 1e-3,
        PhysicalUnits = 1e-9,
        Version = 600,
    };

    file.Structures.Add(new GdsStructure
    {
        CreationTime = DateTime.Now,
        ModificationTime = DateTime.Now,
        Name = $"Structure",
        Elements = GenerateElements(num).ToList(),
    });
    return file;
}

static IEnumerable<GdsElement> GenerateElements(int num)
{
    var rnd = new Random();
    for (var i = 0; i < num; i++)
    {
        var x = rnd.Next(1, 100);
        var y = rnd.Next(1, 100);
        yield return new GdsElement
        {
            Element = new GdsBoxElement
            {
                Layer = 1,
                BoxType = 1,
                Points = new List<GdsPoint>
                {
                    new(x, y),
                    new(x, y + 1),
                    new(x + 1, y + 1),
                    new(x + 1, y),
                    new(x, y)
                }
            }
        };
    }
}