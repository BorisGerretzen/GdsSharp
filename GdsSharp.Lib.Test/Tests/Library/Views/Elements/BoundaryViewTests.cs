using GdsSharp.Lib.Library.Views.Elements;
using GdsSharp.Lib.Test.Helpers;

namespace GdsSharp.Lib.Test.Library.Views.Elements;

[TestFixture]
public class BoundaryViewTests
{
    [Test]
    public void Properties_AreForwarded()
    {
        var (lib, ex) = TestLibraryFactory.Create();
        var view = new BoundaryView(lib, ex.TopBoundary1ElementIndex);

        Assert.Multiple(() =>
        {
            Assert.That(view.ElementIndex, Is.EqualTo(ex.TopBoundary1ElementIndex));
            Assert.That(view.Layer, Is.EqualTo(ex.TopBoundaryLayer));
            Assert.That(view.DataType, Is.EqualTo(ex.TopBoundaryDataType));
            Assert.That(view.PointCount, Is.EqualTo(ex.TopBoundary1Points.Length));
        });
    }

    [Test]
    public void CopyPoints_ThrowsWhenBufferTooSmall()
    {
        var (lib, ex) = TestLibraryFactory.Create();
        var view = new BoundaryView(lib, ex.TopBoundary1ElementIndex);

        var buf = new GdsPoint[view.PointCount - 1];
        Assert.That(() => view.CopyPoints(buf), Throws.ArgumentException);
    }

    [Test]
    public void GetPoints_ReturnsExactPointSequence()
    {
        var (lib, ex) = TestLibraryFactory.Create();
        var view = new BoundaryView(lib, ex.TopBoundary1ElementIndex);

        using var pts = view.GetPoints();
        Assert.That(pts.Length, Is.EqualTo(ex.TopBoundary1Points.Length));
        Assert.That(pts.Span.ToArray(), Is.EqualTo(ex.TopBoundary1Points));
    }
}
