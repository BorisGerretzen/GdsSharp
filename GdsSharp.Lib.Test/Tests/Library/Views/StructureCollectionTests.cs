using GdsSharp.Lib.Library.Views;
using GdsSharp.Lib.Test.Helpers;

namespace GdsSharp.Lib.Test.Library.Views;

[TestFixture]
public class StructureCollectionTests
{
    [Test]
    public void Count_MatchesUnderlyingLibrary()
    {
        var (lib, _) = TestLibraryFactory.Create();
        var view = new LibraryView(lib);

        Assert.That(view.Structures.Count, Is.EqualTo(lib.Structures.Length));
    }

    [Test]
    public void Indexer_ReturnsStableViews()
    {
        var (lib, ex) = TestLibraryFactory.Create();
        var view = new LibraryView(lib);

        var top1 = view.Structures[ex.TopStructureIndex];
        var top2 = view.Structures[ex.TopStructureIndex];

        Assert.Multiple(() =>
        {
            Assert.That(top1.Index, Is.EqualTo(ex.TopStructureIndex));
            Assert.That(top1.Name, Is.EqualTo(ex.TopName));
            Assert.That(top2.Index, Is.EqualTo(ex.TopStructureIndex));
            Assert.That(top2.Name, Is.EqualTo(ex.TopName));
        });
    }
}