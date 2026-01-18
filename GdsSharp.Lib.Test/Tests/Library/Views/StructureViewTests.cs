using GdsSharp.Lib.Library.Views;
using GdsSharp.Lib.Library.Views.Elements;
using GdsSharp.Lib.Reading;
using GdsSharp.Lib.Test.Helpers;

namespace GdsSharp.Lib.Test.Library.Views;

[TestFixture]
public class StructureViewTests
{
    [Test]
    public void Name_IsForwarded()
    {
        var (lib, ex) = TestLibraryFactory.Create();
        var view = new StructureView(lib, ex.TopStructureIndex);
        Assert.That(view.Name, Is.EqualTo(ex.TopName));
    }

    [Test]
    public void Elements_EnumeratesAllElementsInStructure_InInsertionOrder()
    {
        var (lib, ex) = TestLibraryFactory.Create();
        var view = new StructureView(lib, ex.TopStructureIndex);

        var ids = new List<int>();
        foreach (var ev in view.Elements)
            ids.Add(ev.ElementIndex);

        Assert.That(ids, Is.EqualTo(new[]
        {
            ex.TopBoundary1ElementIndex,
            ex.TopPathElementIndex,
            ex.TopBoxElementIndex,
            ex.TopNodeElementIndex,
            ex.TopTextElementIndex,
            ex.TopSrefElementIndex,
            ex.TopArefElementIndex,
            ex.TopBoundary2ElementIndex
        }));
    }
    
    [Test]
    public void TypedCollections_ReturnExpectedCountsForMixedStructure()
    {
        var (lib, ex) = TestLibraryFactory.Create();
        var view = new StructureView(lib, ex.TopStructureIndex);

        Assert.Multiple(() =>
        {
            Assert.That(Count(view.Boundaries), Is.EqualTo(2));
            Assert.That(Count(view.Paths), Is.EqualTo(1));
            Assert.That(Count(view.Boxes), Is.EqualTo(1));
            Assert.That(Count(view.Nodes), Is.EqualTo(1));
            Assert.That(Count(view.Texts), Is.EqualTo(1));
            Assert.That(Count(view.StructureReferences), Is.EqualTo(1));
            Assert.That(Count(view.ArrayReferences), Is.EqualTo(1));
        });
    }

    [Test]
    public void EmptyStructure_HasNoElementsInAnyCollection()
    {
        var (lib, ex) = TestLibraryFactory.Create();
        var view = new StructureView(lib, ex.EmptyStructureIndex);

        Assert.Multiple(() =>
        {
            Assert.That(Count(view.Elements), Is.EqualTo(0));
            Assert.That(Count(view.Boundaries), Is.EqualTo(0));
            Assert.That(Count(view.Paths), Is.EqualTo(0));
            Assert.That(Count(view.Boxes), Is.EqualTo(0));
            Assert.That(Count(view.Nodes), Is.EqualTo(0));
            Assert.That(Count(view.Texts), Is.EqualTo(0));
            Assert.That(Count(view.StructureReferences), Is.EqualTo(0));
            Assert.That(Count(view.ArrayReferences), Is.EqualTo(0));
        });
    }

    private static int Count(ElementCollection collection) { var c = 0; foreach (var _ in collection) c++; return c; }
    private static int Count(BoundaryCollection collection) { var c = 0; foreach (var _ in collection) c++; return c; }
    private static int Count(PathCollection collection) { var c = 0; foreach (var _ in collection) c++; return c; }
    private static int Count(BoxCollection collection) { var c = 0; foreach (var _ in collection) c++; return c; }
    private static int Count(NodeCollection collection) { var c = 0; foreach (var _ in collection) c++; return c; }
    private static int Count(TextCollection collection) { var c = 0; foreach (var _ in collection) c++; return c; }
    private static int Count(StructureReferenceCollection collection) { var c = 0; foreach (var _ in collection) c++; return c; }
    private static int Count(ArrayReferenceCollection collection) { var c = 0; foreach (var _ in collection) c++; return c; }
}
