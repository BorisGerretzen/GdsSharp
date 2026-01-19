using GdsSharp.Lib.Library.Editing;
using GdsSharp.Lib.Reading;

namespace GdsSharp.Lib.Test.Editing;

[TestFixture]
public class EditSessionSetFieldsAndPropertiesTests
{
    [Test]
    public void SetLayerAndDataType_OnBoundaryAndPath_UpdatesPayload()
    {
        var sample = EditingTestHelpers.CreateSampleLibrary();

        using var session = sample.Lib.Edit();

        session.SetLayer(ElementKey.Base(sample.TopBoundaryId), layer: 9);
        session.SetDataType(ElementKey.Base(sample.TopBoundaryId), dataType: 99);

        session.SetLayer(ElementKey.Base(sample.TopPathId), layer: 8);
        session.SetDataType(ElementKey.Base(sample.TopPathId), dataType: 88);

        var newLib = session.Build();

        var boundaryId = EditingTestHelpers.FindFirstElementId(newLib, sample.Top, eid =>
        {
            if (newLib.Elements[eid].Kind != GdsElementKind.Boundary) return false;
            var p = newLib.Boundaries[newLib.Elements[eid].Index];
            return p is { Layer: 9, DataType: 99 };
        });
        Assert.That(boundaryId, Is.Not.EqualTo(-1));
        Assert.That(EditingTestHelpers.ReadGeometryPoints(newLib, boundaryId), Is.EqualTo(sample.TopBoundaryPoints).AsCollection);

        var pathId = EditingTestHelpers.FindFirstElementId(newLib, sample.Top, eid =>
        {
            if (newLib.Elements[eid].Kind != GdsElementKind.Path) return false;
            var p = newLib.Paths[newLib.Elements[eid].Index];
            return p is { Layer: 8, DataType: 88 } && p.Width == sample.TopPathWidth;
        });
        Assert.That(pathId, Is.Not.EqualTo(-1));
        Assert.That(EditingTestHelpers.ReadGeometryPoints(newLib, pathId), Is.EqualTo(sample.TopPathPoints).AsCollection);
    }

    [Test]
    public void SetText_UpdatesTextPayload()
    {
        var sample = EditingTestHelpers.CreateSampleLibrary();

        using var session = sample.Lib.Edit();
        session.SetText(ElementKey.Base(sample.TopTextId), "WORLD");

        var newLib = session.Build();

        var textId = EditingTestHelpers.FindFirstElementId(newLib, sample.Top, eid =>
        {
            if (newLib.Elements[eid].Kind != GdsElementKind.Text) return false;
            var p = newLib.Texts[newLib.Elements[eid].Index];
            return p is { Layer: 5, TextType: 50, Text: "WORLD" };
        });

        Assert.That(textId, Is.Not.EqualTo(-1));
        var tp = newLib.Texts[newLib.Elements[textId].Index];
        Assert.That(tp.Origin, Is.EqualTo(sample.TopTextOrigin));
    }

    [Test]
    public void SetProperty_BaseElement_OverridesAndRemoves()
    {
        var sample = EditingTestHelpers.CreateSampleLibrary();

        using var session = sample.Lib.Edit();

        // base boundary had (123 -> "base")
        session.SetProperty(ElementKey.Base(sample.TopBoundaryId), attribute: 123, value: "updated");
        session.SetProperty(ElementKey.Base(sample.TopBoundaryId), attribute: 555, value: "foo");
        session.RemoveProperty(ElementKey.Base(sample.TopBoundaryId), attribute: 123);

        var newLib = session.Build();

        var boundaryId = EditingTestHelpers.FindFirstElementId(newLib, sample.Top, eid =>
        {
            if (newLib.Elements[eid].Kind != GdsElementKind.Boundary) return false;
            var p = newLib.Boundaries[newLib.Elements[eid].Index];
            return p is { Layer: 1, DataType: 10 };
        });
        Assert.That(boundaryId, Is.Not.EqualTo(-1));

        var props = EditingTestHelpers.GetElementProperties(newLib, boundaryId);
        Assert.That(props.Any(p => p.Attribute == 123), Is.False);
        Assert.That(props.Single(p => p.Attribute == 555).Value, Is.EqualTo("foo"));
    }

    [Test]
    public void SetProperty_NewElement_Persists_AndRemoveWorks()
    {
        var sample = EditingTestHelpers.CreateSampleLibrary();

        using var session = sample.Lib.Edit();

        var pts = new[]
        {
            new GdsPoint(500, 500),
            new GdsPoint(510, 500),
            new GdsPoint(510, 510),
            new GdsPoint(500, 510),
            new GdsPoint(500, 500)
        };

        var ek = session.CreateBoundary(sample.Top, layer: 77, dataType: 7, points: pts);
        session.SetProperty(ek, attribute: 1, value: "a");
        session.SetProperty(ek, attribute: 2, value: "b");
        session.RemoveProperty(ek, attribute: 1);

        var newLib = session.Build();

        var newBoundaryId = EditingTestHelpers.FindFirstElementId(newLib, sample.Top, eid =>
        {
            if (newLib.Elements[eid].Kind != GdsElementKind.Boundary) return false;
            var p = newLib.Boundaries[newLib.Elements[eid].Index];
            return p is { Layer: 77, DataType: 7 };
        });

        Assert.That(newBoundaryId, Is.Not.EqualTo(-1));

        var props = EditingTestHelpers.GetElementProperties(newLib, newBoundaryId);
        Assert.That(props.Any(p => p.Attribute == 1), Is.False);
        Assert.That(props.Single(p => p.Attribute == 2).Value, Is.EqualTo("b"));
    }

    [Test]
    public void SetDataType_OnUnsupportedKind_Throws()
    {
        var sample = EditingTestHelpers.CreateSampleLibrary();
        using var session = sample.Lib.Edit();

        Assert.Throws<InvalidOperationException>(() =>
            session.SetDataType(ElementKey.Base(sample.TopBoxId), dataType: 1));
    }
}
