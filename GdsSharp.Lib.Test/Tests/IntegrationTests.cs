using System.Reflection;
using GdsSharp.Lib.Library;
using GdsSharp.Lib.Library.VertexStore;
using GdsSharp.Lib.Reading;
using GdsSharp.Lib.Reading.Consumer;
using GdsSharp.Lib.Reading.Enum;
using GdsSharp.Lib.Reading.Models;
using GdsSharp.Lib.Reading.TokenStream;
using GdsSharp.Lib.Writing;

namespace GdsSharp.Lib.Test;

public class IntegrationTests
{
    private static readonly string[] RoundtripFiles =
    [
        "example.cal",
        "inv.gds2",
        "nand2.gds2",
        "xor.gds2",
        "gds3d_example.gds"
    ];

    private static readonly int[] BufferSizes =
    [
        8, 31, 33, GdsGlobals.DefaultReaderBufferSize
    ];

    private static IEnumerable<TestCaseData> RoundtripFileAndBufferSizeCases()
    {
        foreach (var file in RoundtripFiles)
        foreach (var size in BufferSizes)
            yield return new TestCaseData(file, size)
                .SetName($"{file}, BufferSize={size}");
    }
    
    [TestCaseSource(nameof(RoundtripFileAndBufferSizeCases))]
    public void TestWriterRoundtrip(string manifestFile, int bufferSize)
    {
        using var streamIn = new MemoryStream();
        using var streamOut = new MemoryStream();

        using var fileStream =
            Assembly.GetExecutingAssembly().GetManifestResourceStream($"GdsSharp.Lib.Test.Assets.{manifestFile}") ??
            throw new NullReferenceException();
        fileStream.CopyTo(streamIn);
        fileStream.Position = 0;

        using var tokenStream = new GdsTokenStream(fileStream, bufferSize: bufferSize);
        var vertexStore = new ChunkedVertexStore();
        var consumer = new GdsLibraryBuilderConsumer(vertexStore);
        var parser = new GdsParser(tokenStream);
        parser.Parse(consumer);

        var writer = new GdsWriter(streamOut);
        writer.Write(consumer.Library);

        // remove padding
        var bytesIn = streamIn.ToArray();
        var paddingLength = bytesIn.Reverse().TakeWhile(e => e == 0).Count() - 1;
        bytesIn = bytesIn.SkipLast(paddingLength).ToArray();

        var bytesOut = streamOut.ToArray();

        // File.WriteAllBytes($"new_{manifestFile}", bytesOut);

        Assert.That(bytesOut, Has.Length.EqualTo(bytesIn.Length));
        Assert.That(bytesOut, Is.EqualTo(bytesIn));
    }

    #region Multiple Elements in Structure Tests

    [Test]
    public void TestMultipleElementsInStructure_AllArePreserved()
    {
        var builder = CreateMinimalLibrary();

        // Add multiple element types
        builder.AddBoundary(null, 1, 0, new GdsPoint[] { new(0, 0), new(100, 0), new(100, 100), new(0, 100), new(0, 0) });
        builder.AddPath(null, 2, 0, new GdsPoint[] { new(200, 0), new(300, 100) }, 10, null);
        builder.AddBox(null, 3, 0, new GdsPoint[] { new(400, 0), new(500, 0), new(500, 100), new(400, 100), new(400, 0) });
        builder.AddNode(null, 4, 0, new GdsPoint[] { new(600, 50) });
        builder.AddText(null, 5, 0, null, null, null, null, new GdsPoint(700, 50), "TestLabel");

        var library = builder.Build();
        var (readLibrary, _) = WriteAndReadBack(library);

        Assert.That(readLibrary.Boundaries, Has.Length.EqualTo(1));
        Assert.That(readLibrary.Paths, Has.Length.EqualTo(1));
        Assert.That(readLibrary.Boxes, Has.Length.EqualTo(1));
        Assert.That(readLibrary.Nodes, Has.Length.EqualTo(1));
        Assert.That(readLibrary.Texts, Has.Length.EqualTo(1));

        Assert.That(readLibrary.Boundaries[0].Layer, Is.EqualTo(1));
        Assert.That(readLibrary.Paths[0].Layer, Is.EqualTo(2));
        Assert.That(readLibrary.Boxes[0].Layer, Is.EqualTo(3));
        Assert.That(readLibrary.Nodes[0].Layer, Is.EqualTo(4));
        Assert.That(readLibrary.Texts[0].Layer, Is.EqualTo(5));
    }

    #endregion

    #region Helper Methods

    /// <summary>
    ///     Writes a library to a stream, then reads it back using the parser.
    /// </summary>
    private static (GdsLibrary Library, ChunkedVertexStore VertexStore) WriteAndReadBack(GdsLibrary library)
    {
        using var stream = new MemoryStream();
        var writer = new GdsWriter(stream);
        writer.Write(library);

        stream.Position = 0;

        using var tokenStream = new GdsTokenStream(stream);
        var readVertexStore = new ChunkedVertexStore();
        var consumer = new GdsLibraryBuilderConsumer(readVertexStore);
        var parser = new GdsParser(tokenStream);
        parser.Parse(consumer);

        return (consumer.Library, readVertexStore);
    }

    /// <summary>
    ///     Creates a minimal library with one structure.
    /// </summary>
    private static GdsLibraryBuilder CreateMinimalLibrary(
        string libraryName = "TestLibrary",
        string structureName = "TestStructure")
    {
        var vertexStore = new ChunkedVertexStore();
        var builder = new GdsLibraryBuilder(vertexStore)
        {
            Info = GdsLibraryInfo.Default with
            {
                Name = libraryName,
                ModificationTime = new DateTime(2025, 1, 1, 12, 0, 0),
                AccessTime = new DateTime(2025, 1, 1, 12, 0, 0)
            }
        };
        builder.AddStructure(new GdsStructureInfo(
            structureName,
            new DateTime(2025, 1, 1, 12, 0, 0),
            new DateTime(2025, 1, 1, 12, 0, 0)
        ));
        return builder;
    }

    /// <summary>
    ///     Reads points from a vertex store at the given offset.
    /// </summary>
    private static GdsPoint[] ReadPoints(IGdsVertexStore store, long offset, int count)
    {
        var points = new GdsPoint[count];
        store.Read(offset, points);
        return points;
    }

    #endregion

    #region Library Info Tests

    [Test]
    public void TestLibraryInfo_VersionIsPreserved()
    {
        var builder = CreateMinimalLibrary();
        builder.Info = builder.Info!.Value with { Version = 700 };

        // Add a dummy boundary so there's at least one element
        builder.AddBoundary(null, 1, 0, new GdsPoint[] { new(0, 0), new(100, 0), new(100, 100), new(0, 100), new(0, 0) });

        var library = builder.Build();
        var (readLibrary, _) = WriteAndReadBack(library);

        Assert.That(readLibrary.Info.Version, Is.EqualTo(700));
    }

    [Test]
    public void TestLibraryInfo_NameIsPreserved()
    {
        var builder = CreateMinimalLibrary("MyTestLibrary");
        builder.AddBoundary(null, 1, 0, new GdsPoint[] { new(0, 0), new(100, 0), new(100, 100), new(0, 100), new(0, 0) });

        var library = builder.Build();
        var (readLibrary, _) = WriteAndReadBack(library);

        Assert.That(readLibrary.Info.Name, Is.EqualTo("MyTestLibrary"));
    }

    [Test]
    public void TestLibraryInfo_TimestampsArePreserved()
    {
        var modTime = new DateTime(2024, 6, 15, 10, 30, 45);
        var accessTime = new DateTime(2024, 6, 16, 11, 45, 30);

        var builder = CreateMinimalLibrary();
        builder.Info = builder.Info!.Value with
        {
            ModificationTime = modTime,
            AccessTime = accessTime
        };
        builder.AddBoundary(null, 1, 0, new GdsPoint[] { new(0, 0), new(100, 0), new(100, 100), new(0, 100), new(0, 0) });

        var library = builder.Build();
        var (readLibrary, _) = WriteAndReadBack(library);

        Assert.That(readLibrary.Info.ModificationTime, Is.EqualTo(modTime));
        Assert.That(readLibrary.Info.AccessTime, Is.EqualTo(accessTime));
    }

    [Test]
    public void TestLibraryInfo_UnitsArePreserved()
    {
        var builder = CreateMinimalLibrary();
        builder.Info = builder.Info!.Value with
        {
            UserUnits = 0.001,
            PhysicalUnits = 1e-9
        };
        builder.AddBoundary(null, 1, 0, new GdsPoint[] { new(0, 0), new(100, 0), new(100, 100), new(0, 100), new(0, 0) });

        var library = builder.Build();
        var (readLibrary, _) = WriteAndReadBack(library);

        Assert.That(readLibrary.Info.UserUnits, Is.EqualTo(0.001).Within(1e-12));
        Assert.That(readLibrary.Info.PhysicalUnits, Is.EqualTo(1e-9).Within(1e-18));
    }

    [Test]
    public void TestLibraryInfo_GenerationsIsPreserved()
    {
        var builder = CreateMinimalLibrary();
        builder.Info = builder.Info!.Value with { Generations = 5 };
        builder.AddBoundary(null, 1, 0, new GdsPoint[] { new(0, 0), new(100, 0), new(100, 100), new(0, 100), new(0, 0) });

        var library = builder.Build();
        var (readLibrary, _) = WriteAndReadBack(library);

        Assert.That(readLibrary.Info.Generations, Is.EqualTo(5));
    }

    [Test]
    public void TestLibraryInfo_FormatTypeIsPreserved()
    {
        var builder = CreateMinimalLibrary();
        builder.Info = builder.Info!.Value with { FormatType = GdsFormatType.GdsArchive };
        builder.AddBoundary(null, 1, 0, new GdsPoint[] { new(0, 0), new(100, 0), new(100, 100), new(0, 100), new(0, 0) });

        var library = builder.Build();
        var (readLibrary, _) = WriteAndReadBack(library);

        Assert.That(readLibrary.Info.FormatType, Is.EqualTo(GdsFormatType.GdsArchive));
    }

    #endregion

    #region Structure Tests

    [Test]
    public void TestStructure_NameIsPreserved()
    {
        var builder = CreateMinimalLibrary(structureName: "MyStructure");
        builder.AddBoundary(null, 1, 0, new GdsPoint[] { new(0, 0), new(100, 0), new(100, 100), new(0, 100), new(0, 0) });

        var library = builder.Build();
        var (readLibrary, _) = WriteAndReadBack(library);

        Assert.That(readLibrary.Structures, Has.Length.EqualTo(1));
        Assert.That(readLibrary.Structures[0].Info.Name, Is.EqualTo("MyStructure"));
    }

    [Test]
    public void TestStructure_TimestampsArePreserved()
    {
        var creationTime = new DateTime(2024, 3, 10, 8, 15, 20);
        var modificationTime = new DateTime(2024, 3, 11, 9, 20, 25);

        var vertexStore = new ChunkedVertexStore();
        var builder = new GdsLibraryBuilder(vertexStore)
        {
            Info = GdsLibraryInfo.Default with { Name = "TestLib" }
        };
        builder.AddStructure(new GdsStructureInfo(
            "TestStruct",
            creationTime,
            modificationTime
        ));
        builder.AddBoundary(null, 1, 0, new GdsPoint[] { new(0, 0), new(100, 0), new(100, 100), new(0, 100), new(0, 0) });

        var library = builder.Build();
        var (readLibrary, _) = WriteAndReadBack(library);

        Assert.That(readLibrary.Structures[0].Info.CreationTime, Is.EqualTo(creationTime));
        Assert.That(readLibrary.Structures[0].Info.ModificationTime, Is.EqualTo(modificationTime));
    }

    [Test]
    public void TestMultipleStructures_AllArePreserved()
    {
        var vertexStore = new ChunkedVertexStore();
        var builder = new GdsLibraryBuilder(vertexStore)
        {
            Info = GdsLibraryInfo.Default with { Name = "TestLib" }
        };

        builder.AddStructure(new GdsStructureInfo("Structure1", DateTime.Now, DateTime.Now));
        builder.AddBoundary(null, 1, 0, new GdsPoint[] { new(0, 0), new(100, 0), new(100, 100), new(0, 100), new(0, 0) });

        builder.AddStructure(new GdsStructureInfo("Structure2", DateTime.Now, DateTime.Now));
        builder.AddBoundary(null, 2, 0, new GdsPoint[] { new(200, 200), new(300, 200), new(300, 300), new(200, 300), new(200, 200) });

        builder.AddStructure(new GdsStructureInfo("Structure3", DateTime.Now, DateTime.Now));
        builder.AddBoundary(null, 3, 0, new GdsPoint[] { new(400, 400), new(500, 400), new(500, 500), new(400, 500), new(400, 400) });

        var library = builder.Build();
        var (readLibrary, _) = WriteAndReadBack(library);

        Assert.That(readLibrary.Structures, Has.Length.EqualTo(3));
        Assert.That(readLibrary.Structures[0].Info.Name, Is.EqualTo("Structure1"));
        Assert.That(readLibrary.Structures[1].Info.Name, Is.EqualTo("Structure2"));
        Assert.That(readLibrary.Structures[2].Info.Name, Is.EqualTo("Structure3"));
    }

    #endregion

    #region Boundary Element Tests

    [Test]
    public void TestBoundary_LayerAndDataTypeArePreserved()
    {
        var builder = CreateMinimalLibrary();
        builder.AddBoundary(null, 5, 10,
            new GdsPoint[] { new(0, 0), new(100, 0), new(100, 100), new(0, 100), new(0, 0) });

        var library = builder.Build();
        var (readLibrary, _) = WriteAndReadBack(library);

        Assert.That(readLibrary.Boundaries, Has.Length.EqualTo(1));
        Assert.That(readLibrary.Boundaries[0].Layer, Is.EqualTo(5));
        Assert.That(readLibrary.Boundaries[0].DataType, Is.EqualTo(10));
    }

    [Test]
    public void TestBoundary_VerticesArePreserved()
    {
        var builder = CreateMinimalLibrary();
        var originalPoints = new GdsPoint[]
        {
            new(0, 0), new(1000, 0), new(1000, 500), new(500, 500), new(500, 1000), new(0, 1000), new(0, 0)
        };
        builder.AddBoundary(null, 1, 0, originalPoints);

        var library = builder.Build();
        var (readLibrary, readVertexStore) = WriteAndReadBack(library);

        var boundary = readLibrary.Boundaries[0];
        var readPoints = ReadPoints(readVertexStore, boundary.VertexOffset, boundary.VertexCount);

        Assert.That(readPoints, Is.EqualTo(originalPoints));
    }

    [Test]
    public void TestBoundary_WithElementCommon()
    {
        var builder = CreateMinimalLibrary();
        var common = new GdsElementCommon(true, false, 42);
        builder.AddBoundary(common, 1, 0,
            new GdsPoint[] { new(0, 0), new(100, 0), new(100, 100), new(0, 100), new(0, 0) });

        var library = builder.Build();
        var (readLibrary, _) = WriteAndReadBack(library);

        var element = readLibrary.Elements[0];
        Assert.That(element.Common, Is.Not.Null);
        Assert.That(element.Common!.Value.ExternalData, Is.True);
        Assert.That(element.Common!.Value.TemplateData, Is.False);
        Assert.That(element.Common!.Value.PlexNumber, Is.EqualTo(42));
    }

    #endregion

    #region Path Element Tests

    [Test]
    public void TestPath_LayerAndDataTypeArePreserved()
    {
        var builder = CreateMinimalLibrary();
        builder.AddPath(null, 7, 3,
            new GdsPoint[] { new(0, 0), new(100, 0), new(100, 100) },
            null, null);

        var library = builder.Build();
        var (readLibrary, _) = WriteAndReadBack(library);

        Assert.That(readLibrary.Paths, Has.Length.EqualTo(1));
        Assert.That(readLibrary.Paths[0].Layer, Is.EqualTo(7));
        Assert.That(readLibrary.Paths[0].DataType, Is.EqualTo(3));
    }

    [Test]
    public void TestPath_WidthIsPreserved()
    {
        var builder = CreateMinimalLibrary();
        builder.AddPath(null, 1, 0,
            new GdsPoint[] { new(0, 0), new(100, 0), new(100, 100) },
            50, null);

        var library = builder.Build();
        var (readLibrary, _) = WriteAndReadBack(library);

        Assert.That(readLibrary.Paths[0].Width, Is.EqualTo(50));
    }

    [Test]
    public void TestPath_PathTypeIsPreserved()
    {
        var builder = CreateMinimalLibrary();
        builder.AddPath(null, 1, 0,
            new GdsPoint[] { new(0, 0), new(100, 0), new(100, 100) },
            10, GdsPathType.Rounded);

        var library = builder.Build();
        var (readLibrary, _) = WriteAndReadBack(library);

        Assert.That(readLibrary.Paths[0].PathType, Is.EqualTo(GdsPathType.Rounded));
    }

    [TestCase(GdsPathType.Square)]
    [TestCase(GdsPathType.Rounded)]
    [TestCase(GdsPathType.SquareExtended)]
    public void TestPath_AllPathTypesArePreserved(GdsPathType pathType)
    {
        var builder = CreateMinimalLibrary();
        builder.AddPath(null, 1, 0,
            new GdsPoint[] { new(0, 0), new(100, 0) },
            10, pathType);

        var library = builder.Build();
        var (readLibrary, _) = WriteAndReadBack(library);

        Assert.That(readLibrary.Paths[0].PathType, Is.EqualTo(pathType));
    }

    [Test]
    public void TestPath_VerticesArePreserved()
    {
        var builder = CreateMinimalLibrary();
        var originalPoints = new GdsPoint[] { new(0, 0), new(500, 250), new(1000, 0), new(1500, 250) };
        builder.AddPath(null, 1, 0, originalPoints, 10, null);

        var library = builder.Build();
        var (readLibrary, readVertexStore) = WriteAndReadBack(library);

        var path = readLibrary.Paths[0];
        var readPoints = ReadPoints(readVertexStore, path.VertexOffset, path.VertexCount);

        Assert.That(readPoints, Is.EqualTo(originalPoints));
    }

    #endregion

    #region Box Element Tests

    [Test]
    public void TestBox_LayerAndBoxTypeArePreserved()
    {
        var builder = CreateMinimalLibrary();
        builder.AddBox(null, 4, 8,
            new GdsPoint[] { new(0, 0), new(100, 0), new(100, 100), new(0, 100), new(0, 0) });

        var library = builder.Build();
        var (readLibrary, _) = WriteAndReadBack(library);

        Assert.That(readLibrary.Boxes, Has.Length.EqualTo(1));
        Assert.That(readLibrary.Boxes[0].Layer, Is.EqualTo(4));
        Assert.That(readLibrary.Boxes[0].BoxType, Is.EqualTo(8));
    }

    [Test]
    public void TestBox_VerticesArePreserved()
    {
        var builder = CreateMinimalLibrary();
        var originalPoints = new GdsPoint[] { new(10, 20), new(110, 20), new(110, 120), new(10, 120), new(10, 20) };
        builder.AddBox(null, 1, 0, originalPoints);

        var library = builder.Build();
        var (readLibrary, readVertexStore) = WriteAndReadBack(library);

        var box = readLibrary.Boxes[0];
        var readPoints = ReadPoints(readVertexStore, box.VertexOffset, GdsGlobals.BoxPointCount);

        Assert.That(readPoints, Is.EqualTo(originalPoints));
    }

    #endregion

    #region Node Element Tests

    [Test]
    public void TestNode_LayerAndNodeTypeArePreserved()
    {
        var builder = CreateMinimalLibrary();
        builder.AddNode(null, 6, 12,
            new GdsPoint[] { new(0, 0), new(50, 50), new(100, 0) });

        var library = builder.Build();
        var (readLibrary, _) = WriteAndReadBack(library);

        Assert.That(readLibrary.Nodes, Has.Length.EqualTo(1));
        Assert.That(readLibrary.Nodes[0].Layer, Is.EqualTo(6));
        Assert.That(readLibrary.Nodes[0].NodeType, Is.EqualTo(12));
    }

    [Test]
    public void TestNode_VerticesArePreserved()
    {
        var builder = CreateMinimalLibrary();
        var originalPoints = new GdsPoint[] { new(100, 200), new(300, 400), new(500, 600) };
        builder.AddNode(null, 1, 0, originalPoints);

        var library = builder.Build();
        var (readLibrary, readVertexStore) = WriteAndReadBack(library);

        var node = readLibrary.Nodes[0];
        var readPoints = ReadPoints(readVertexStore, node.VertexOffset, node.VertexCount);

        Assert.That(readPoints, Is.EqualTo(originalPoints));
    }

    #endregion

    #region Text Element Tests

    [Test]
    public void TestText_LayerAndTextTypeArePreserved()
    {
        var builder = CreateMinimalLibrary();
        builder.AddText(null, 9, 15, null, null, null,
            null, new GdsPoint(100, 200), "Hello");

        var library = builder.Build();
        var (readLibrary, _) = WriteAndReadBack(library);

        Assert.That(readLibrary.Texts, Has.Length.EqualTo(1));
        Assert.That(readLibrary.Texts[0].Layer, Is.EqualTo(9));
        Assert.That(readLibrary.Texts[0].TextType, Is.EqualTo(15));
    }

    [Test]
    public void TestText_TextStringIsPreserved()
    {
        var builder = CreateMinimalLibrary();
        builder.AddText(null, 1, 0, null, null, null, null, new GdsPoint(0, 0), "Test String 123!");

        var library = builder.Build();
        var (readLibrary, _) = WriteAndReadBack(library);

        Assert.That(readLibrary.Texts[0].Text, Is.EqualTo("Test String 123!"));
    }

    [Test]
    public void TestText_OriginIsPreserved()
    {
        var builder = CreateMinimalLibrary();
        builder.AddText(null, 1, 0, null, null, null, null, new GdsPoint(500, 750), "Origin Test");

        var library = builder.Build();
        var (readLibrary, _) = WriteAndReadBack(library);

        Assert.That(readLibrary.Texts[0].Origin, Is.EqualTo(new GdsPoint(500, 750)));
    }

    [Test]
    public void TestText_PresentationIsPreserved()
    {
        var builder = CreateMinimalLibrary();
        var presentation = new PresentationInfo(2, 1, 2);
        builder.AddText(null, 1, 0, presentation, null, null, null, new GdsPoint(0, 0), "Presentation Test");

        var library = builder.Build();
        var (readLibrary, _) = WriteAndReadBack(library);

        Assert.That(readLibrary.Texts[0].Presentation, Is.Not.Null);
        Assert.That(readLibrary.Texts[0].Presentation!.Value.Font, Is.EqualTo(2));
        Assert.That(readLibrary.Texts[0].Presentation!.Value.HorizontalJustification, Is.EqualTo(1));
        Assert.That(readLibrary.Texts[0].Presentation!.Value.VerticalJustification, Is.EqualTo(2));
    }

    [Test]
    public void TestText_PathTypeIsPreserved()
    {
        var builder = CreateMinimalLibrary();
        builder.AddText(null, 1, 0, null, GdsPathType.SquareExtended, null, null, new GdsPoint(0, 0), "PathType Test");

        var library = builder.Build();
        var (readLibrary, _) = WriteAndReadBack(library);

        Assert.That(readLibrary.Texts[0].PathType, Is.EqualTo(GdsPathType.SquareExtended));
    }

    [Test]
    public void TestText_WidthIsPreserved()
    {
        var builder = CreateMinimalLibrary();
        builder.AddText(null, 1, 0, null, null, 25, null, new GdsPoint(0, 0), "Width Test");

        var library = builder.Build();
        var (readLibrary, _) = WriteAndReadBack(library);

        Assert.That(readLibrary.Texts[0].Width, Is.EqualTo(25));
    }

    [Test]
    public void TestText_StransIsPreserved()
    {
        var builder = CreateMinimalLibrary();
        var strans = new GdsStransInfo(true, true, false,
            2.5, 45.0);
        builder.AddText(null, 1, 0, null, null, null, strans, new GdsPoint(0, 0), "Strans Test");

        var library = builder.Build();
        var (readLibrary, _) = WriteAndReadBack(library);

        Assert.That(readLibrary.Texts[0].Strans, Is.Not.Null);
        Assert.That(readLibrary.Texts[0].Strans!.Value.Reflection, Is.True);
        Assert.That(readLibrary.Texts[0].Strans!.Value.AbsoluteMagnification, Is.True);
        Assert.That(readLibrary.Texts[0].Strans!.Value.AbsoluteAngle, Is.False);
        Assert.That(readLibrary.Texts[0].Strans!.Value.Magnification, Is.EqualTo(2.5).Within(1e-9));
        Assert.That(readLibrary.Texts[0].Strans!.Value.Angle, Is.EqualTo(45.0).Within(1e-9));
    }

    [Test]
    public void TestText_OddLengthStringIsPadded()
    {
        var builder = CreateMinimalLibrary();
        // "ABC" is 3 characters (odd length)
        builder.AddText(null, 1, 0, null, null, null, null, new GdsPoint(0, 0), "ABC");

        var library = builder.Build();
        var (readLibrary, _) = WriteAndReadBack(library);

        // Should still read back correctly (padding is transparent)
        Assert.That(readLibrary.Texts[0].Text, Is.EqualTo("ABC"));
    }

    #endregion

    #region SRef Element Tests

    [Test]
    public void TestSRef_TargetNameIsPreserved()
    {
        var vertexStore = new ChunkedVertexStore();
        var builder = new GdsLibraryBuilder(vertexStore)
        {
            Info = GdsLibraryInfo.Default with { Name = "TestLib" }
        };

        // Create target structure
        builder.AddStructure(new GdsStructureInfo("TargetCell", DateTime.Now, DateTime.Now));
        builder.AddBoundary(null, 1, 0, new GdsPoint[] { new(0, 0), new(10, 0), new(10, 10), new(0, 10), new(0, 0) });

        // Create parent structure with SRef
        builder.AddStructure(new GdsStructureInfo("ParentCell", DateTime.Now, DateTime.Now));
        builder.AddStructureReference(null, "TargetCell", null, new GdsPoint(100, 200));

        var library = builder.Build();
        var (readLibrary, _) = WriteAndReadBack(library);

        Assert.That(readLibrary.StructureReferences, Has.Length.EqualTo(1));
        Assert.That(readLibrary.StructureReferences[0].TargetName, Is.EqualTo("TargetCell"));
    }

    [Test]
    public void TestSRef_OriginIsPreserved()
    {
        var vertexStore = new ChunkedVertexStore();
        var builder = new GdsLibraryBuilder(vertexStore)
        {
            Info = GdsLibraryInfo.Default with { Name = "TestLib" }
        };

        builder.AddStructure(new GdsStructureInfo("TargetCell", DateTime.Now, DateTime.Now));
        builder.AddBoundary(null, 1, 0, new GdsPoint[] { new(0, 0), new(10, 0), new(10, 10), new(0, 10), new(0, 0) });

        builder.AddStructure(new GdsStructureInfo("ParentCell", DateTime.Now, DateTime.Now));
        builder.AddStructureReference(null, "TargetCell", null, new GdsPoint(500, 750));

        var library = builder.Build();
        var (readLibrary, _) = WriteAndReadBack(library);

        Assert.That(readLibrary.StructureReferences[0].Origin, Is.EqualTo(new GdsPoint(500, 750)));
    }

    [Test]
    public void TestSRef_StransIsPreserved()
    {
        var vertexStore = new ChunkedVertexStore();
        var builder = new GdsLibraryBuilder(vertexStore)
        {
            Info = GdsLibraryInfo.Default with { Name = "TestLib" }
        };

        builder.AddStructure(new GdsStructureInfo("TargetCell", DateTime.Now, DateTime.Now));
        builder.AddBoundary(null, 1, 0, new GdsPoint[] { new(0, 0), new(10, 0), new(10, 10), new(0, 10), new(0, 0) });

        builder.AddStructure(new GdsStructureInfo("ParentCell", DateTime.Now, DateTime.Now));
        var strans = new GdsStransInfo(true, false, true,
            1.5, 90.0);
        builder.AddStructureReference(null, "TargetCell", strans, new GdsPoint(0, 0));

        var library = builder.Build();
        var (readLibrary, _) = WriteAndReadBack(library);

        Assert.That(readLibrary.StructureReferences[0].Strans, Is.Not.Null);
        Assert.That(readLibrary.StructureReferences[0].Strans!.Value.Reflection, Is.True);
        Assert.That(readLibrary.StructureReferences[0].Strans!.Value.AbsoluteMagnification, Is.False);
        Assert.That(readLibrary.StructureReferences[0].Strans!.Value.AbsoluteAngle, Is.True);
        Assert.That(readLibrary.StructureReferences[0].Strans!.Value.Magnification, Is.EqualTo(1.5).Within(1e-9));
        Assert.That(readLibrary.StructureReferences[0].Strans!.Value.Angle, Is.EqualTo(90.0).Within(1e-9));
    }

    #endregion

    #region ARef Element Tests

    [Test]
    public void TestARef_TargetNameIsPreserved()
    {
        var vertexStore = new ChunkedVertexStore();
        var builder = new GdsLibraryBuilder(vertexStore)
        {
            Info = GdsLibraryInfo.Default with { Name = "TestLib" }
        };

        builder.AddStructure(new GdsStructureInfo("ArrayCell", DateTime.Now, DateTime.Now));
        builder.AddBoundary(null, 1, 0, new GdsPoint[] { new(0, 0), new(10, 0), new(10, 10), new(0, 10), new(0, 0) });

        builder.AddStructure(new GdsStructureInfo("ParentCell", DateTime.Now, DateTime.Now));
        builder.AddArrayReference(null, "ArrayCell", null, 3, 4,
            new GdsPoint(0, 100), new GdsPoint(100, 0), new GdsPoint(0, 0));

        var library = builder.Build();
        var (readLibrary, _) = WriteAndReadBack(library);

        Assert.That(readLibrary.ArrayReferences, Has.Length.EqualTo(1));
        Assert.That(readLibrary.ArrayReferences[0].TargetName, Is.EqualTo("ArrayCell"));
    }

    [Test]
    public void TestARef_RowsAndColumnsArePreserved()
    {
        var vertexStore = new ChunkedVertexStore();
        var builder = new GdsLibraryBuilder(vertexStore)
        {
            Info = GdsLibraryInfo.Default with { Name = "TestLib" }
        };

        builder.AddStructure(new GdsStructureInfo("ArrayCell", DateTime.Now, DateTime.Now));
        builder.AddBoundary(null, 1, 0, new GdsPoint[] { new(0, 0), new(10, 0), new(10, 10), new(0, 10), new(0, 0) });

        builder.AddStructure(new GdsStructureInfo("ParentCell", DateTime.Now, DateTime.Now));
        builder.AddArrayReference(null, "ArrayCell", null, 5, 8,
            new GdsPoint(0, 50), new GdsPoint(50, 0), new GdsPoint(0, 0));

        var library = builder.Build();
        var (readLibrary, _) = WriteAndReadBack(library);

        Assert.That(readLibrary.ArrayReferences[0].Rows, Is.EqualTo(5));
        Assert.That(readLibrary.ArrayReferences[0].Columns, Is.EqualTo(8));
    }

    [Test]
    public void TestARef_VectorsAndOriginArePreserved()
    {
        var vertexStore = new ChunkedVertexStore();
        var builder = new GdsLibraryBuilder(vertexStore)
        {
            Info = GdsLibraryInfo.Default with { Name = "TestLib" }
        };

        builder.AddStructure(new GdsStructureInfo("ArrayCell", DateTime.Now, DateTime.Now));
        builder.AddBoundary(null, 1, 0, new GdsPoint[] { new(0, 0), new(10, 0), new(10, 10), new(0, 10), new(0, 0) });

        builder.AddStructure(new GdsStructureInfo("ParentCell", DateTime.Now, DateTime.Now));
        builder.AddArrayReference(null, "ArrayCell", null, 2, 3,
            new GdsPoint(0, 200), new GdsPoint(150, 0), new GdsPoint(1000, 2000));

        var library = builder.Build();
        var (readLibrary, _) = WriteAndReadBack(library);

        Assert.That(readLibrary.ArrayReferences[0].Origin, Is.EqualTo(new GdsPoint(1000, 2000)));
        Assert.That(readLibrary.ArrayReferences[0].RowVector, Is.EqualTo(new GdsPoint(0, 200)));
        Assert.That(readLibrary.ArrayReferences[0].ColumnVector, Is.EqualTo(new GdsPoint(150, 0)));
    }

    [Test]
    public void TestARef_StransIsPreserved()
    {
        var vertexStore = new ChunkedVertexStore();
        var builder = new GdsLibraryBuilder(vertexStore)
        {
            Info = GdsLibraryInfo.Default with { Name = "TestLib" }
        };

        builder.AddStructure(new GdsStructureInfo("ArrayCell", DateTime.Now, DateTime.Now));
        builder.AddBoundary(null, 1, 0, new GdsPoint[] { new(0, 0), new(10, 0), new(10, 10), new(0, 10), new(0, 0) });

        builder.AddStructure(new GdsStructureInfo("ParentCell", DateTime.Now, DateTime.Now));
        var strans = new GdsStransInfo(false, true, true,
            0.5, 180.0);
        builder.AddArrayReference(null, "ArrayCell", strans, 2, 2,
            new GdsPoint(0, 100), new GdsPoint(100, 0), new GdsPoint(0, 0));

        var library = builder.Build();
        var (readLibrary, _) = WriteAndReadBack(library);

        Assert.That(readLibrary.ArrayReferences[0].Strans, Is.Not.Null);
        Assert.That(readLibrary.ArrayReferences[0].Strans!.Value.Reflection, Is.False);
        Assert.That(readLibrary.ArrayReferences[0].Strans!.Value.AbsoluteMagnification, Is.True);
        Assert.That(readLibrary.ArrayReferences[0].Strans!.Value.AbsoluteAngle, Is.True);
        Assert.That(readLibrary.ArrayReferences[0].Strans!.Value.Magnification, Is.EqualTo(0.5).Within(1e-9));
        Assert.That(readLibrary.ArrayReferences[0].Strans!.Value.Angle, Is.EqualTo(180.0).Within(1e-9));
    }

    #endregion

    #region Property Tests

    [Test]
    public void TestProperty_AttributeAndValueArePreserved()
    {
        var builder = CreateMinimalLibrary();
        var elementId = builder.AddBoundary(null, 1, 0,
            new GdsPoint[] { new(0, 0), new(100, 0), new(100, 100), new(0, 100), new(0, 0) });
        builder.AddElementProperty(elementId, 1, "PropertyValue123");

        var library = builder.Build();
        var (readLibrary, _) = WriteAndReadBack(library);

        Assert.That(readLibrary.Properties, Has.Length.EqualTo(1));
        Assert.That(readLibrary.Properties[0].Attribute, Is.EqualTo(1));
        Assert.That(readLibrary.Properties[0].Value, Is.EqualTo("PropertyValue123"));
    }

    [Test]
    public void TestProperty_MultiplePropertiesOnSameElement()
    {
        var builder = CreateMinimalLibrary();
        var elementId = builder.AddBoundary(null, 1, 0,
            new GdsPoint[] { new(0, 0), new(100, 0), new(100, 100), new(0, 100), new(0, 0) });
        builder.AddElementProperty(elementId, 1, "First");
        builder.AddElementProperty(elementId, 2, "Second");
        builder.AddElementProperty(elementId, 3, "Third");

        var library = builder.Build();
        var (readLibrary, _) = WriteAndReadBack(library);

        Assert.That(readLibrary.Properties, Has.Length.EqualTo(3));

        var props = readLibrary.Properties.OrderBy(p => p.Attribute).ToArray();
        Assert.That(props[0].Attribute, Is.EqualTo(1));
        Assert.That(props[0].Value, Is.EqualTo("First"));
        Assert.That(props[1].Attribute, Is.EqualTo(2));
        Assert.That(props[1].Value, Is.EqualTo("Second"));
        Assert.That(props[2].Attribute, Is.EqualTo(3));
        Assert.That(props[2].Value, Is.EqualTo("Third"));
    }

    [Test]
    public void TestProperty_PropertiesOnDifferentElements()
    {
        var builder = CreateMinimalLibrary();

        var elementId1 = builder.AddBoundary(null, 1, 0,
            new GdsPoint[] { new(0, 0), new(100, 0), new(100, 100), new(0, 100), new(0, 0) });
        builder.AddElementProperty(elementId1, 10, "BoundaryProp");

        var elementId2 = builder.AddPath(null, 2, 0,
            new GdsPoint[] { new(0, 0), new(100, 100) }, 5, null);
        builder.AddElementProperty(elementId2, 20, "PathProp");

        var library = builder.Build();
        var (readLibrary, _) = WriteAndReadBack(library);

        Assert.That(readLibrary.Properties, Has.Length.EqualTo(2));

        var boundaryProp = readLibrary.Properties.First(p => p.Attribute == 10);
        var pathProp = readLibrary.Properties.First(p => p.Attribute == 20);

        Assert.That(boundaryProp.Value, Is.EqualTo("BoundaryProp"));
        Assert.That(pathProp.Value, Is.EqualTo("PathProp"));
    }

    #endregion

    #region ElementCommon Tests

    [Test]
    public void TestElementCommon_ElementFlagsArePreserved()
    {
        var builder = CreateMinimalLibrary();
        var common = new GdsElementCommon(true, true, null);
        builder.AddBoundary(common, 1, 0,
            new GdsPoint[] { new(0, 0), new(100, 0), new(100, 100), new(0, 100), new(0, 0) });

        var library = builder.Build();
        var (readLibrary, _) = WriteAndReadBack(library);

        var element = readLibrary.Elements[0];
        Assert.That(element.Common!.Value.ExternalData, Is.True);
        Assert.That(element.Common!.Value.TemplateData, Is.True);
    }

    [Test]
    public void TestElementCommon_PlexNumberIsPreserved()
    {
        var builder = CreateMinimalLibrary();
        var common = new GdsElementCommon(null, null, 12345);
        builder.AddBoundary(common, 1, 0,
            new GdsPoint[] { new(0, 0), new(100, 0), new(100, 100), new(0, 100), new(0, 0) });

        var library = builder.Build();
        var (readLibrary, _) = WriteAndReadBack(library);

        var element = readLibrary.Elements[0];
        Assert.That(element.Common!.Value.PlexNumber, Is.EqualTo(12345));
    }

    #endregion
}