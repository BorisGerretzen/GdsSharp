using GdsSharp.Lib.Library.Views;
using GdsSharp.Lib.Library.Views.Elements;
using GdsSharp.Lib.Test.Helpers;

namespace GdsSharp.Lib.Test.Library.Views.Elements;

[TestFixture]
public class ArrayReferenceCollectionTests
{
    [Test]
    public void Enumerator_ReturnsOnlyArrayReferences()
    {
        var (lib, ex) = TestLibraryFactory.Create();
        var structure = new StructureView(lib, ex.TopStructureIndex);

        var ids = new List<int>();
        foreach (var r in structure.ArrayReferences)
            ids.Add(r.ElementIndex);

        Assert.That(ids, Is.EqualTo(new[] { ex.TopArefElementIndex }));
    }

    [Test]
    public void StructureIndex_IsPreserved()
    {
        var (lib, ex) = TestLibraryFactory.Create();
        var c = new ArrayReferenceCollection(lib, ex.TopStructureIndex);
        Assert.That(c.StructureIndex, Is.EqualTo(ex.TopStructureIndex));
    }
}