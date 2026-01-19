using GdsSharp.Lib.Library.Views.Elements;
using GdsSharp.Lib.Test.Helpers;

namespace GdsSharp.Lib.Test.Library.Views.Elements;

[TestFixture]
public class NodeViewTests
{
    [Test]
    public void Properties_AreForwarded()
    {
        var (lib, ex) = TestLibraryFactory.Create();
        var view = new NodeView(lib, ex.TopNodeElementIndex);

        Assert.Multiple(() =>
        {
            Assert.That(view.ElementIndex, Is.EqualTo(ex.TopNodeElementIndex));
            Assert.That(view.Layer, Is.EqualTo(ex.TopNodeLayer));
            Assert.That(view.NodeType, Is.EqualTo(ex.TopNodeDataType));
            Assert.That(view.PointCount, Is.EqualTo(ex.TopNodePoints.Length));
        });
    }

    [Test]
    public void CopyPoints_ThrowsWhenBufferTooSmall()
    {
        var (lib, ex) = TestLibraryFactory.Create();
        var view = new NodeView(lib, ex.TopNodeElementIndex);

        var buf = new GdsPoint[view.PointCount - 1];
        Assert.That(() => view.CopyPoints(buf), Throws.ArgumentException);
    }

    [Test]
    public void GetPoints_ReturnsExactPointSequence()
    {
        var (lib, ex) = TestLibraryFactory.Create();
        var view = new NodeView(lib, ex.TopNodeElementIndex);

        using var pts = view.GetPoints();
        Assert.That(pts.Span.ToArray(), Is.EqualTo(ex.TopNodePoints));
    }
}