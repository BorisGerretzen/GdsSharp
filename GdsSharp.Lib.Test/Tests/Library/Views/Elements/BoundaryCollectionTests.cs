using GdsSharp.Lib.Library.Views;
using GdsSharp.Lib.Library.Views.Elements;
using GdsSharp.Lib.Test.Helpers;

namespace GdsSharp.Lib.Test.Library.Views.Elements;

[TestFixture]
public class BoundaryCollectionTests
{
    [Test]
    public void Enumerator_ReturnsOnlyBoundaryElements_InOrder()
    {
        var (lib, ex) = TestLibraryFactory.Create();
        var structure = new StructureView(lib, ex.TopStructureIndex);

        var ids = new List<int>();
        foreach (var b in structure.Boundaries)
            ids.Add(b.ElementIndex);

        Assert.That(ids, Is.EqualTo(new[] { ex.TopBoundary1ElementIndex, ex.TopBoundary2ElementIndex }));
    }

    [Test]
    public void StructureIndex_IsPreserved()
    {
        var (lib, ex) = TestLibraryFactory.Create();
        var c = new BoundaryCollection(lib, ex.TopStructureIndex);
        Assert.That(c.StructureIndex, Is.EqualTo(ex.TopStructureIndex));
    }
}
