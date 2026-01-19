using GdsSharp.Lib.Library.Views;
using GdsSharp.Lib.Reading;
using GdsSharp.Lib.Test.Helpers;

namespace GdsSharp.Lib.Test.Library.Views;

[TestFixture]
public class ElementViewTests
{
    [Test]
    public void Kind_And_Layer_AreForwarded()
    {
        var (lib, ex) = TestLibraryFactory.Create();
        var top = new StructureView(lib, ex.TopStructureIndex);

        // Boundary
        var b = new ElementView(lib, ex.TopBoundary1ElementIndex);
        Assert.That(b.Kind, Is.EqualTo(GdsElementKind.Boundary));
        Assert.That(b.Layer, Is.EqualTo(ex.TopBoundaryLayer));

        // Path
        var p = new ElementView(lib, ex.TopPathElementIndex);
        Assert.That(p.Kind, Is.EqualTo(GdsElementKind.Path));
        Assert.That(p.Layer, Is.EqualTo(6));

        // References have no layers by design
        var sref = new ElementView(lib, ex.TopSrefElementIndex);
        Assert.That(sref.Kind, Is.EqualTo(GdsElementKind.StructureReference));
        Assert.That(sref.Layer, Is.EqualTo(-1));

        var aref = new ElementView(lib, ex.TopArefElementIndex);
        Assert.That(aref.Kind, Is.EqualTo(GdsElementKind.ArrayReference));
        Assert.That(aref.Layer, Is.EqualTo(-1));
    }

    [Test]
    public void AsX_ReturnsViewsBoundToSameElementIndex()
    {
        var (lib, ex) = TestLibraryFactory.Create();

        var boundary = new ElementView(lib, ex.TopBoundary1ElementIndex).AsBoundary();
        Assert.That(boundary.ElementIndex, Is.EqualTo(ex.TopBoundary1ElementIndex));

        var path = new ElementView(lib, ex.TopPathElementIndex).AsPath();
        Assert.That(path.ElementIndex, Is.EqualTo(ex.TopPathElementIndex));

        var box = new ElementView(lib, ex.TopBoxElementIndex).AsBox();
        Assert.That(box.ElementIndex, Is.EqualTo(ex.TopBoxElementIndex));

        var node = new ElementView(lib, ex.TopNodeElementIndex).AsNode();
        Assert.That(node.ElementIndex, Is.EqualTo(ex.TopNodeElementIndex));

        var text = new ElementView(lib, ex.TopTextElementIndex).AsText();
        Assert.That(text.ElementIndex, Is.EqualTo(ex.TopTextElementIndex));

        var sref = new ElementView(lib, ex.TopSrefElementIndex).AsStructureReference();
        Assert.That(sref.ElementIndex, Is.EqualTo(ex.TopSrefElementIndex));

        var aref = new ElementView(lib, ex.TopArefElementIndex).AsArrayReference();
        Assert.That(aref.ElementIndex, Is.EqualTo(ex.TopArefElementIndex));
    }
}