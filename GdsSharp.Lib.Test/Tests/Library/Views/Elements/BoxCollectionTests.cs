using GdsSharp.Lib.Library.Views;
using GdsSharp.Lib.Library.Views.Elements;
using GdsSharp.Lib.Test.Helpers;

namespace GdsSharp.Lib.Test.Library.Views.Elements;

[TestFixture]
public class BoxCollectionTests
{
    [Test]
    public void Enumerator_ReturnsOnlyBoxElements()
    {
        var (lib, ex) = TestLibraryFactory.Create();
        var structure = new StructureView(lib, ex.TopStructureIndex);

        var ids = new List<int>();
        foreach (var b in structure.Boxes)
            ids.Add(b.ElementIndex);

        Assert.That(ids, Is.EqualTo(new[] { ex.TopBoxElementIndex }));
    }

    [Test]
    public void StructureIndex_IsPreserved()
    {
        var (lib, ex) = TestLibraryFactory.Create();
        var c = new BoxCollection(lib, ex.TopStructureIndex);
        Assert.That(c.StructureIndex, Is.EqualTo(ex.TopStructureIndex));
    }
}
