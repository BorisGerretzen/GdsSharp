using System.Reflection;
using FluentAssertions;
using GdsSharp.Lib.Obsolete;
using GdsSharp.Lib.Obsolete.Lexing;
using GdsSharp.Lib.Reading;
using GdsSharp.Lib.Reading.Consumer;
using GdsSharp.Lib.Reading.TokenStream;

namespace GdsSharp.Lib.Test;

public class IntegrationTests
{
    [TestCase("example.cal")]
    [TestCase("inv.gds2")]
    [TestCase("nand2.gds2")]
    [TestCase("xor.gds2")]
    [TestCase("gds3d_example.gds")]
    public void TestRoundTrip(string manifestFile)
    {
        using var fileStream =
            Assembly.GetExecutingAssembly().GetManifestResourceStream($"GdsSharp.Lib.Test.Assets.{manifestFile}") ??
            throw new NullReferenceException();
        using var tokenStream = new GdsTokenStream(fileStream);
        var parser = new GdsParser(tokenStream);
        var file = parser.Parse();

        using var ms = new MemoryStream();
        file.WriteTo(ms);
        ms.Position = 0;

        using var tokenStreamNew = new GdsTokenStream(ms);
        var parserNew = new GdsParser(tokenStreamNew);
        var fileNew = parserNew.Parse();

        file.Materialize();
        fileNew.Materialize();

        file.Should().BeEquivalentTo(fileNew);
    }

    [TestCase("example.cal")]
    [TestCase("inv.gds2")]
    [TestCase("nand2.gds2")]
    [TestCase("xor.gds2")]
    [TestCase("gds3d_example.gds")]
    public void TestOutputEquality(string manifestFile)
    {
        byte[] bytesOld;
        byte[] bytesNew;

        using (var fileStream =
               Assembly.GetExecutingAssembly().GetManifestResourceStream($"GdsSharp.Lib.Test.Assets.{manifestFile}") ??
               throw new NullReferenceException())
        {
            using var tokenStream = new GdsTokenStream(fileStream);
            var parser = new GdsParser(tokenStream);
            var file = parser.Parse();
            file.Materialize();
            using var ms = new MemoryStream();
            file.WriteTo(ms);
            bytesOld = ms.ToArray();
        }

        Console.WriteLine("NEW");
        using (var fileStream =
               Assembly.GetExecutingAssembly().GetManifestResourceStream($"GdsSharp.Lib.Test.Assets.{manifestFile}") ??
               throw new NullReferenceException())
        {
            using var tokenStream = new NewGdsTokenStream(fileStream);
            var parser = new NewGdsParser(tokenStream);
            var consumer = new OldParserConsumer();
            parser.Parse(consumer);
            var file = consumer.File;
            using var ms = new MemoryStream();
            file.WriteTo(ms);
            bytesNew = ms.ToArray();
        }

        bytesNew.Should().BeEquivalentTo(bytesOld);
    }

    [TestCase("example.cal")]
    [TestCase("inv.gds2")]
    [TestCase("nand2.gds2")]
    [TestCase("xor.gds2")]
    [TestCase("gds3d_example.gds")]
    public void TestEquality(string manifestFile)
    {
        GdsFile old;
        GdsFile @new;
        using (var fileStream =
               Assembly.GetExecutingAssembly().GetManifestResourceStream($"GdsSharp.Lib.Test.Assets.{manifestFile}") ??
               throw new NullReferenceException())
        {
            using var tokenStream = new GdsTokenStream(fileStream);
            var parser = new GdsParser(tokenStream);
            old = parser.Parse();
            old.Materialize();
        }

        using (var fileStream =
               Assembly.GetExecutingAssembly().GetManifestResourceStream($"GdsSharp.Lib.Test.Assets.{manifestFile}") ??
               throw new NullReferenceException())
        {
            using var tokenStream = new NewGdsTokenStream(fileStream);
            var parser = new NewGdsParser(tokenStream);
            var consumer = new OldParserConsumer();
            parser.Parse(consumer);
            @new = consumer.File;
        }

        Assert.That(@new.Structures.Count(), Is.EqualTo(old.Structures.Count()));
        @new.Should().BeEquivalentTo(old);

        @new.Version.Should().Be(old.Version);
        @new.LibraryName.Should().Be(old.LibraryName);
        @new.LastModificationTime.Should().Be(old.LastModificationTime);
        @new.LastAccessTime.Should().Be(old.LastAccessTime);
        @new.PhysicalUnits.Should().Be(old.PhysicalUnits);
        @new.UserUnits.Should().Be(old.UserUnits);
        @new.ReferencedLibraries.Should().BeEquivalentTo(old.ReferencedLibraries);
        @new.Fonts.Should().BeEquivalentTo(old.Fonts);
        @new.Generations.Should().Be(old.Generations);
        @new.FormatType.Should().Be(old.FormatType);

        if (old.Structures.Any())
        {
            var sOld = old.Structures.First();
            var sNew = @new.Structures.First();

            sNew.Name.Should().Be(sOld.Name);
            sNew.CreationTime.Should().Be(sOld.CreationTime);
            sNew.ModificationTime.Should().Be(sOld.ModificationTime);
            sNew.Should().BeEquivalentTo(sOld);

            foreach (var (eOld, eNew) in sOld.Elements.Zip(sNew.Elements))
            {
                eNew.Should().BeEquivalentTo(eOld);

                eNew.Properties.Should().BeEquivalentTo(eOld.Properties);
                eNew.Element.Should().BeEquivalentTo(eOld.Element);
            }
        }
    }
}