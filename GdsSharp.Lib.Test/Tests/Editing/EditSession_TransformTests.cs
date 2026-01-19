using GdsSharp.Lib.Library.Editing;
using GdsSharp.Lib.Reading;

namespace GdsSharp.Lib.Test.Editing;

[TestFixture]
public class EditSessionTransformTests
{
    [Test]
    public void Transform_Translate_Boundary_UpdatesVertices()
    {
        var sample = EditingTestHelpers.CreateSampleLibrary();
        using var session = sample.Lib.Edit();

        session.Transform(ElementKey.Base(sample.TopBoundaryId), TransformOp.Translate, dx: 5, dy: -7);

        var newLib = session.Build();

        var boundaryId = EditingTestHelpers.FindFirstElementId(newLib, sample.Top, eid =>
        {
            if (newLib.Elements[eid].Kind != GdsElementKind.Boundary) return false;
            var p = newLib.Boundaries[newLib.Elements[eid].Index];
            return p is { Layer: 1, DataType: 10 };
        });
        Assert.That(boundaryId, Is.Not.EqualTo(-1));

        var expected = sample.TopBoundaryPoints.Select(p => new GdsPoint(p.X + 5, p.Y - 7)).ToArray();
        Assert.That(EditingTestHelpers.ReadGeometryPoints(newLib, boundaryId), Is.EqualTo(expected).AsCollection);
    }

    [Test]
    public void Transform_Rotate90CW_Boundary_AboutOrigin_UpdatesVertices()
    {
        var sample = EditingTestHelpers.CreateSampleLibrary();
        using var session = sample.Lib.Edit();

        session.Transform(ElementKey.Base(sample.TopBoundaryId), TransformOp.Rotate90Clockwise, pivot: new GdsPoint(0, 0));

        var newLib = session.Build();

        var boundaryId = EditingTestHelpers.FindFirstElementId(newLib, sample.Top, eid =>
        {
            if (newLib.Elements[eid].Kind != GdsElementKind.Boundary) return false;
            var p = newLib.Boundaries[newLib.Elements[eid].Index];
            return p is { Layer: 1, DataType: 10 };
        });
        Assert.That(boundaryId, Is.Not.EqualTo(-1));

        var expected = sample.TopBoundaryPoints.Select(p => Rotate90Cw(p, new GdsPoint(0, 0))).ToArray();
        Assert.That(EditingTestHelpers.ReadGeometryPoints(newLib, boundaryId), Is.EqualTo(expected).AsCollection);
    }

    [Test]
    public void Transform_MirrorX_Boundary_AboutOrigin_UpdatesVertices()
    {
        var sample = EditingTestHelpers.CreateSampleLibrary();
        using var session = sample.Lib.Edit();

        session.Transform(ElementKey.Base(sample.TopBoundaryId), TransformOp.MirrorX, pivot: new GdsPoint(0, 0));

        var newLib = session.Build();

        var boundaryId = EditingTestHelpers.FindFirstElementId(newLib, sample.Top, eid =>
        {
            if (newLib.Elements[eid].Kind != GdsElementKind.Boundary) return false;
            var p = newLib.Boundaries[newLib.Elements[eid].Index];
            return p is { Layer: 1, DataType: 10 };
        });
        Assert.That(boundaryId, Is.Not.EqualTo(-1));

        var expected = sample.TopBoundaryPoints.Select(p => new GdsPoint(p.X, -p.Y)).ToArray();
        Assert.That(EditingTestHelpers.ReadGeometryPoints(newLib, boundaryId), Is.EqualTo(expected).AsCollection);
    }

    [Test]
    public void Transform_Text_Rotate90CW_UpdatesOrigin_AndStrans()
    {
        var sample = EditingTestHelpers.CreateSampleLibrary();
        using var session = sample.Lib.Edit();

        session.Transform(ElementKey.Base(sample.TopTextId), TransformOp.Rotate90Clockwise, pivot: new GdsPoint(0, 0));

        var newLib = session.Build();

        var textId = EditingTestHelpers.FindFirstElementId(newLib, sample.Top, eid => newLib.Elements[eid].Kind == GdsElementKind.Text);
        Assert.That(textId, Is.Not.EqualTo(-1));

        var tp = newLib.Texts[newLib.Elements[textId].Index];

        Assert.That(tp.Text, Is.EqualTo(sample.TopText));
        Assert.That(tp.Origin, Is.EqualTo(Rotate90Cw(sample.TopTextOrigin, new GdsPoint(0, 0))));

        Assert.That(tp.Strans.HasValue, Is.True);
        Assert.That(tp.Strans!.Value.Reflection, Is.False);
        Assert.That(tp.Strans!.Value.Angle, Is.EqualTo(90.0));
        Assert.That(tp.Strans!.Value.Magnification, Is.EqualTo(1.0));
    }

    [Test]
    public void Transform_SRef_MirrorY_UpdatesOrigin_AndStrans()
    {
        var sample = EditingTestHelpers.CreateSampleLibrary();
        using var session = sample.Lib.Edit();

        session.Transform(ElementKey.Base(sample.TopSRefId), TransformOp.MirrorY, pivot: new GdsPoint(0, 0));

        var newLib = session.Build();

        var srefId = EditingTestHelpers.FindFirstElementId(newLib, sample.Top, eid =>
        {
            if (newLib.Elements[eid].Kind != GdsElementKind.StructureReference) return false;
            var p = newLib.StructureReferences[newLib.Elements[eid].Index];
            return p.TargetName == sample.RefTargetName;
        });
        Assert.That(srefId, Is.Not.EqualTo(-1));

        var sp = newLib.StructureReferences[newLib.Elements[srefId].Index];

        // mirrorY about origin -> x becomes -x
        Assert.That(sp.Origin, Is.EqualTo(new GdsPoint(-sample.SRefOrigin.X, sample.SRefOrigin.Y)));

        Assert.That(sp.Strans.HasValue, Is.True);
        Assert.That(sp.Strans!.Value.Reflection, Is.True);
        Assert.That(sp.Strans!.Value.Angle, Is.EqualTo(180.0));
    }

    [Test]
    public void Transform_ARef_Rotate90CCW_UpdatesAllPoints_AndStrans()
    {
        var sample = EditingTestHelpers.CreateSampleLibrary();
        using var session = sample.Lib.Edit();

        session.Transform(ElementKey.Base(sample.TopARefId), TransformOp.Rotate90CounterClockwise, pivot: new GdsPoint(0, 0));

        var newLib = session.Build();

        var arefId = EditingTestHelpers.FindFirstElementId(newLib, sample.Top, eid =>
        {
            if (newLib.Elements[eid].Kind != GdsElementKind.ArrayReference) return false;
            var p = newLib.ArrayReferences[newLib.Elements[eid].Index];
            return p.TargetName == sample.RefTargetName && p.Rows == sample.ARefRows && p.Columns == sample.ARefColumns;
        });
        Assert.That(arefId, Is.Not.EqualTo(-1));

        var ap = newLib.ArrayReferences[newLib.Elements[arefId].Index];

        Assert.That(ap.Origin, Is.EqualTo(Rotate90Ccw(sample.ARefOrigin, new GdsPoint(0, 0))));
        Assert.That(ap.RowVector, Is.EqualTo(Rotate90Ccw(sample.ARefRowVector, new GdsPoint(0, 0))));
        Assert.That(ap.ColumnVector, Is.EqualTo(Rotate90Ccw(sample.ARefColVector, new GdsPoint(0, 0))));

        Assert.That(ap.Strans.HasValue, Is.True);
        Assert.That(ap.Strans!.Value.Angle, Is.EqualTo(-90.0));
    }

    private static GdsPoint Rotate90Cw(GdsPoint p, GdsPoint pivot)
    {
        var x = p.X - pivot.X;
        var y = p.Y - pivot.Y;
        return new GdsPoint(pivot.X + y, pivot.Y - x);
    }

    private static GdsPoint Rotate90Ccw(GdsPoint p, GdsPoint pivot)
    {
        var x = p.X - pivot.X;
        var y = p.Y - pivot.Y;
        return new GdsPoint(pivot.X - y, pivot.Y + x);
    }
}
