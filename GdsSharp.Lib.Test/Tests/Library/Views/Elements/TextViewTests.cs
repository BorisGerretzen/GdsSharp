using GdsSharp.Lib.Library.Views.Elements;
using GdsSharp.Lib.Test.Helpers;

namespace GdsSharp.Lib.Test.Library.Views.Elements;

[TestFixture]
public class TextViewTests
{
    [Test]
    public void Properties_AreForwarded()
    {
        var (lib, ex) = TestLibraryFactory.Create();
        var view = new TextView(lib, ex.TopTextElementIndex);

        Assert.Multiple(() =>
        {
            Assert.That(view.ElementIndex, Is.EqualTo(ex.TopTextElementIndex));
            Assert.That(view.Layer, Is.EqualTo(ex.TopTextLayer));
            Assert.That(view.TextType, Is.EqualTo(ex.TopTextType));
            Assert.That(view.Origin, Is.EqualTo(ex.TopTextOrigin));
            Assert.That(view.Text, Is.EqualTo(ex.TopText));
            Assert.That(view.Presentation, Is.Null);
            Assert.That(view.PathType, Is.Null);
            Assert.That(view.Width, Is.Null);
            Assert.That(view.Strans, Is.Null);
        });
    }
}