using GdsSharp.Lib.Library.Views.Elements;
using GdsSharp.Lib.Test.Helpers;

namespace GdsSharp.Lib.Test.Library.Views.Elements;

[TestFixture]
public class PathViewTests
{
    [Test]
    public void Properties_AreForwarded()
    {
        var (lib, ex) = TestLibraryFactory.Create();
        var view = new PathView(lib, ex.TopPathElementIndex);

        Assert.Multiple(() =>
        {
            Assert.That(view.ElementIndex, Is.EqualTo(ex.TopPathElementIndex));
            Assert.That(view.Layer, Is.EqualTo(6));
            Assert.That(view.DataType, Is.EqualTo(12));
            Assert.That(view.Width, Is.EqualTo(ex.TopPathWidth));
            Assert.That(view.PathType, Is.EqualTo(ex.TopPathType));
            Assert.That(view.PointCount, Is.EqualTo(ex.TopPathPoints.Length));
        });
    }

    [Test]
    public void CopyPoints_ThrowsWhenBufferTooSmall()
    {
        var (lib, ex) = TestLibraryFactory.Create();
        var view = new PathView(lib, ex.TopPathElementIndex);

        var buf = new GdsPoint[view.PointCount - 1];
        Assert.That(() => view.CopyPoints(buf), Throws.ArgumentException);
    }

    [Test]
    public void GetPoints_ReturnsExactPointSequence()
    {
        var (lib, ex) = TestLibraryFactory.Create();
        var view = new PathView(lib, ex.TopPathElementIndex);

        using var pts = view.GetPoints();
        Assert.That(pts.Span.ToArray(), Is.EqualTo(ex.TopPathPoints));
    }
}