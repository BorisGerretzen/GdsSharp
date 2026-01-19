using GdsSharp.Lib.Library.Views.Elements;
using GdsSharp.Lib.Test.Helpers;

namespace GdsSharp.Lib.Test.Library.Views.Elements;

[TestFixture]
public class StructureReferenceViewTests
{
    [Test]
    public void Properties_AreForwarded()
    {
        var (lib, ex) = TestLibraryFactory.Create();
        var view = new StructureReferenceView(lib, ex.TopSrefElementIndex);

        Assert.Multiple(() =>
        {
            Assert.That(view.ElementIndex, Is.EqualTo(ex.TopSrefElementIndex));
            Assert.That(view.Parent.Id, Is.EqualTo(ex.TopStructureIndex));
            Assert.That(view.TargetName, Is.EqualTo(ex.RefTargetName));
            Assert.That(view.Strans, Is.Null);
            Assert.That(view.Origin, Is.EqualTo(ex.RefOrigin));
        });
    }
}