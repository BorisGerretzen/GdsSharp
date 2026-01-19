using GdsSharp.Lib.Library.Views;
using GdsSharp.Lib.Test.Helpers;

namespace GdsSharp.Lib.Test.Library.Views;

[TestFixture]
public class ElementCollectionTests
{
    [Test]
    public void StructureIndex_IsPreserved()
    {
        var (lib, ex) = TestLibraryFactory.Create();
        var c = new ElementCollection(lib, ex.TopStructureIndex);
        Assert.That(c.StructureIndex, Is.EqualTo(ex.TopStructureIndex));
    }

    [Test]
    public void EnumeratingElements_YieldsExactlyElementCount_AndValidIndices()
    {
        var (lib, ex) = TestLibraryFactory.Create();
        var view = new StructureView(lib, ex.TopStructureIndex);

        var elementIndices = new List<int>();
        foreach (var ev in view.Elements)
        {
            elementIndices.Add(ev.ElementIndex);
            Assert.That(ev.ElementIndex, Is.InRange(0, lib.Elements.Length - 1));
        }

        Assert.That(elementIndices.Count, Is.EqualTo(8));
        Assert.That(elementIndices[0], Is.EqualTo(ex.TopBoundary1ElementIndex));
        Assert.That(elementIndices[^1], Is.EqualTo(ex.TopBoundary2ElementIndex));
    }

    [Test]
    public void Count_IsCorrect()
    {
        var (lib, ex) = TestLibraryFactory.Create();
        var view = new StructureView(lib, ex.TopStructureIndex);
        Assert.That(view.Elements.Count, Is.EqualTo(8));
    }
}