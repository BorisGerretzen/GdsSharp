using GdsSharp.Lib.Library.Views;
using GdsSharp.Lib.Library.Views.Elements;
using GdsSharp.Lib.Test.Helpers;

namespace GdsSharp.Lib.Test.Library.Views.Elements;

[TestFixture]
public class PathCollectionTests
{
    [Test]
    public void Enumerator_ReturnsOnlyPathElements()
    {
        var (lib, ex) = TestLibraryFactory.Create();
        var structure = new StructureView(lib, ex.TopStructureIndex);

        var ids = new List<int>();
        foreach (var p in structure.Paths)
            ids.Add(p.ElementIndex);

        Assert.That(ids, Is.EqualTo(new[] { ex.TopPathElementIndex }));
    }

    [Test]
    public void StructureIndex_IsPreserved()
    {
        var (lib, ex) = TestLibraryFactory.Create();
        var c = new PathCollection(lib, ex.TopStructureIndex);
        Assert.That(c.StructureIndex, Is.EqualTo(ex.TopStructureIndex));
    }
}
