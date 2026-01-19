using GdsSharp.Lib.Library.Views;
using GdsSharp.Lib.Library.Views.Elements;
using GdsSharp.Lib.Test.Helpers;

namespace GdsSharp.Lib.Test.Library.Views.Elements;

[TestFixture]
public class NodeCollectionTests
{
    [Test]
    public void Enumerator_ReturnsOnlyNodeElements()
    {
        var (lib, ex) = TestLibraryFactory.Create();
        var structure = new StructureView(lib, ex.TopStructureIndex);

        var ids = new List<int>();
        foreach (var n in structure.Nodes)
            ids.Add(n.ElementIndex);

        Assert.That(ids, Is.EqualTo(new[] { ex.TopNodeElementIndex }));
    }

    [Test]
    public void StructureIndex_IsPreserved()
    {
        var (lib, ex) = TestLibraryFactory.Create();
        var c = new NodeCollection(lib, ex.TopStructureIndex);
        Assert.That(c.StructureIndex, Is.EqualTo(ex.TopStructureIndex));
    }
}