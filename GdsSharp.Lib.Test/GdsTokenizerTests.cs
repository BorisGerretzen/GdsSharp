using System.Reflection;
using GdsSharp.Lib.Terminals.Records;

namespace GdsSharp.Lib.Test;

public class GdsTokenizerTests
{
    private Stream _stream1 = null!;
    private Stream _stream2 = null!;
    private Stream _stream3 = null!;
    private Stream _stream4 = null!;

    [SetUp]
    public void SetUp()
    {
        _stream1 = Assembly.GetExecutingAssembly().GetManifestResourceStream("GdsSharp.Lib.Test.Assets.example.cal") ??
                   throw new NullReferenceException();
        _stream2 = Assembly.GetExecutingAssembly().GetManifestResourceStream("GdsSharp.Lib.Test.Assets.inv.gds2") ??
                   throw new NullReferenceException();
        _stream3 = Assembly.GetExecutingAssembly().GetManifestResourceStream("GdsSharp.Lib.Test.Assets.nand2.gds2") ??
                   throw new NullReferenceException();
        _stream4 = Assembly.GetExecutingAssembly().GetManifestResourceStream("GdsSharp.Lib.Test.Assets.xor.gds2") ??
                   throw new NullReferenceException();
    }

    [Test]
    public void TokenizeDoesntCrashOnExample1()
    {
        var tokenizer = new GdsTokenizer(_stream1);
        var tokens = tokenizer.Tokenize().ToList();
        Assert.That(tokens, Has.Count.EqualTo(76));

        Assert.That(tokens.FirstOfType<GdsRecordHeader>().Value, Is.EqualTo(5));

        Assert.Multiple(() =>
        {
            var bgnLib = tokens.FirstOfType<GdsRecordBgnLib>();
            Assert.That(bgnLib.LastModificationTimeYear, Is.EqualTo(98));
            Assert.That(bgnLib.LastModificationTimeMonth, Is.EqualTo(8));
            Assert.That(bgnLib.LastModificationTimeDay, Is.EqualTo(25));
            Assert.That(bgnLib.LastModificationTimeHour, Is.EqualTo(15));
            Assert.That(bgnLib.LastModificationTimeMinute, Is.EqualTo(53));
            Assert.That(bgnLib.LastModificationTimeSecond, Is.EqualTo(12));
            Assert.That(bgnLib.LastAccessTimeYear, Is.EqualTo(98));
            Assert.That(bgnLib.LastAccessTimeMonth, Is.EqualTo(8));
            Assert.That(bgnLib.LastAccessTimeDay, Is.EqualTo(25));
            Assert.That(bgnLib.LastAccessTimeHour, Is.EqualTo(15));
            Assert.That(bgnLib.LastAccessTimeMinute, Is.EqualTo(53));
            Assert.That(bgnLib.LastAccessTimeSecond, Is.EqualTo(12));
        });

        Assert.That(tokens.FirstOfType<GdsRecordLibName>().Value, Is.EqualTo("TEMPEGS.DB"));

        Assert.That(tokens.FirstOfType<GdsRecordUnits>().UserUnits, Is.EqualTo(0.01));
        Assert.That(tokens.FirstOfType<GdsRecordUnits>().PhysicalUnits, Is.EqualTo(1e-08));

        Assert.Multiple(() =>
        {
            var bgnStr = tokens.FirstOfType<GdsRecordBgnStr>();
            Assert.That(bgnStr.CreationTimeYear, Is.EqualTo(98));
            Assert.That(bgnStr.CreationTimeMonth, Is.EqualTo(8));
            Assert.That(bgnStr.CreationTimeDay, Is.EqualTo(25));
            Assert.That(bgnStr.CreationTimeHour, Is.EqualTo(15));
            Assert.That(bgnStr.CreationTimeMinute, Is.EqualTo(53));
            Assert.That(bgnStr.CreationTimeSecond, Is.EqualTo(12));
            Assert.That(bgnStr.LastModificationTimeYear, Is.EqualTo(98));
            Assert.That(bgnStr.LastModificationTimeMonth, Is.EqualTo(8));
            Assert.That(bgnStr.LastModificationTimeDay, Is.EqualTo(25));
            Assert.That(bgnStr.LastModificationTimeHour, Is.EqualTo(15));
            Assert.That(bgnStr.LastModificationTimeMinute, Is.EqualTo(53));
            Assert.That(bgnStr.LastModificationTimeSecond, Is.EqualTo(12));
        });

        Assert.That(tokens.FirstOfType<GdsRecordStrName>().Value, Is.EqualTo("AAP"));

        Assert.Multiple(() =>
        {
            var xy = tokens.FirstOfType<GdsRecordXy>();
            Assert.That(xy.Coordinates[0], Is.EqualTo((-920000, 452000)));
            Assert.That(xy.Coordinates[1], Is.EqualTo((656500, 765500)));
            Assert.That(xy.Coordinates[2], Is.EqualTo((175000, -174000)));
            Assert.That(xy.Coordinates[3], Is.EqualTo((-756000, -198000)));
            Assert.That(xy.Coordinates[4], Is.EqualTo((-920000, 452000)));
        });

        Assert.Multiple(() =>
        {
            var strans = tokens.FirstOfType<GdsRecordSTrans>();
            Assert.That(strans.Reflection, Is.False);
            Assert.That(strans.AbsoluteMagnification, Is.False);
            Assert.That(strans.AbsoluteAngle, Is.False);
        });

        Assert.That(tokens.FirstOfType<GdsRecordMag>().Value, Is.EqualTo(1875));

        Assert.Multiple(() =>
        {
            var presentation = tokens.FirstOfType<GdsRecordPresentation>();
            Assert.That(presentation.FontNumber, Is.Zero);
            Assert.That(presentation.VerticalPresentation, Is.EqualTo(2));
            Assert.That(presentation.HorizontalPresentation, Is.Zero);
        });
    }

    [Test]
    public void TokenizeDoesntCrashOnExample2()
    {
        var tokenizer = new GdsTokenizer(_stream2);
        var tokens = tokenizer.Tokenize().ToList();
        Assert.That(tokens, Has.Count.EqualTo(425));
    }

    [Test]
    public void TokenizeDoesntCrashOnExample3()
    {
        var tokenizer = new GdsTokenizer(_stream3);
        var tokens = tokenizer.Tokenize().ToList();
        Assert.That(tokens, Has.Count.EqualTo(1229));
    }

    [Test]
    public void TokenizeDoesntCrashOnExample4()
    {
        var tokenizer = new GdsTokenizer(_stream4);
        var tokens = tokenizer.Tokenize().ToList();
        Assert.That(tokens, Has.Count.EqualTo(1229));
    }
}