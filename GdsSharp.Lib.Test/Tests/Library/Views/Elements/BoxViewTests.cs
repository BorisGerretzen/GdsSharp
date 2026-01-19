using GdsSharp.Lib.Library.Views.Elements;
using GdsSharp.Lib.Test.Helpers;

namespace GdsSharp.Lib.Test.Library.Views.Elements;

[TestFixture]
public class BoxViewTests
{
    [Test]
    public void Properties_AreForwarded()
    {
        var (lib, ex) = TestLibraryFactory.Create();
        var view = new BoxView(lib, ex.TopBoxElementIndex);

        Assert.Multiple(() =>
        {
            Assert.That(view.ElementIndex, Is.EqualTo(ex.TopBoxElementIndex));
            Assert.That(view.Layer, Is.EqualTo(ex.TopBoxLayer));
            Assert.That(view.BoxType, Is.EqualTo(ex.TopBoxDataType));
        });
    }

    [Test]
    public void CopyPoints_ThrowsWhenBufferTooSmall()
    {
        var (lib, ex) = TestLibraryFactory.Create();
        var view = new BoxView(lib, ex.TopBoxElementIndex);

        var buf = new GdsPoint[GdsGlobals.BoxPointCount - 1];
        Assert.That(() => view.CopyPoints(buf), Throws.ArgumentException);
    }

    [Test]
    public void GetPoints_ReturnsExactPointSequence()
    {
        var (lib, ex) = TestLibraryFactory.Create();
        var view = new BoxView(lib, ex.TopBoxElementIndex);

        using var pts = view.GetPoints();
        Assert.That(pts.Span.ToArray(), Is.EqualTo(ex.TopBoxPoints));
    }
}