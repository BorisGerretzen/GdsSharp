using GdsSharp.Lib.Library.Views.Elements;
using GdsSharp.Lib.Test.Helpers;

namespace GdsSharp.Lib.Test.Library.Views.Elements;

[TestFixture]
public class ArrayReferenceViewTests
{
    [Test]
    public void Properties_AreForwarded()
    {
        var (lib, ex) = TestLibraryFactory.Create();
        var view = new ArrayReferenceView(lib, ex.TopArefElementIndex);

        Assert.Multiple(() =>
        {
            Assert.That(view.ElementIndex, Is.EqualTo(ex.TopArefElementIndex));
            Assert.That(view.ParentStructureIndex, Is.EqualTo(ex.TopStructureIndex));
            Assert.That(view.TargetName, Is.EqualTo(ex.RefTargetName));
            Assert.That(view.Strans, Is.Null);
            Assert.That(view.Rows, Is.EqualTo(ex.ArefRows));
            Assert.That(view.Columns, Is.EqualTo(ex.ArefCols));
            Assert.That(view.RowVector, Is.EqualTo(ex.ArefRowVector));
            Assert.That(view.ColumnVector, Is.EqualTo(ex.ArefColVector));
            Assert.That(view.Origin, Is.EqualTo(ex.RefOrigin));
        });
    }
}