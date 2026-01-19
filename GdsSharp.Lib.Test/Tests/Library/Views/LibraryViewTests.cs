using GdsSharp.Lib.Library.Views;
using GdsSharp.Lib.Test.Helpers;

namespace GdsSharp.Lib.Test.Library.Views;

[TestFixture]
public class LibraryViewTests
{
    [Test]
    public void Info_IsForwardedFromLibrary()
    {
        var (lib, ex) = TestLibraryFactory.Create();
        var view = new LibraryView(lib);

        Assert.That(view.Info.Name, Is.EqualTo(ex.LibraryName));
        Assert.That(view.Info.Version, Is.EqualTo(lib.Info.Version));
    }

    [Test]
    public void Structures_CountAndIndexer_Work()
    {
        var (lib, ex) = TestLibraryFactory.Create();
        var view = new LibraryView(lib);

        Assert.That(view.Structures.Count, Is.EqualTo(3));

        Assert.Multiple(() =>
        {
            Assert.That(view.Structures[ex.ChildStructureIndex].Name, Is.EqualTo(ex.ChildName));
            Assert.That(view.Structures[ex.TopStructureIndex].Name, Is.EqualTo(ex.TopName));
            Assert.That(view.Structures[ex.EmptyStructureIndex].Name, Is.EqualTo(ex.EmptyName));
        });
    }

    [Test]
    public void TryGetStructure_ExistingStructure_ReturnsTrueAndStructure()
    {
        var (lib, ex) = TestLibraryFactory.Create();
        var view = new LibraryView(lib);

        var result = view.TryGetStructure(ex.TopName, out var structure);

        Assert.That(result, Is.True);
        Assert.That(structure.Name, Is.EqualTo(ex.TopName));
    }

    [Test]
    public void TryGetStructure_ExistingStructure_DifferentCasing_ReturnsFalse()
    {
        var (lib, ex) = TestLibraryFactory.Create();
        var view = new LibraryView(lib);

        var result = view.TryGetStructure(ex.TopName.ToLower(), out var structure);

        Assert.That(result, Is.False);
    }
}