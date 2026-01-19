using GdsSharp.Lib.Library.BoundingBox;
using GdsSharp.Lib.Library.Editing;
using GdsSharp.Lib.Reading;

namespace GdsSharp.Lib.Test.Editing;

[TestFixture]
public class EditSessionReplacePointsTests
{
    [Test]
    public void ReplacePoints_Boundary_UpdatesVertices_AndBoundingBox()
    {
        var sample = EditingTestHelpers.CreateSampleLibrary();

        var newPts = new[]
        {
            new GdsPoint(5, 5),
            new GdsPoint(25, 5),
            new GdsPoint(25, 15),
            new GdsPoint(5, 15),
            new GdsPoint(5, 5)
        };

        using var session = sample.Lib.Edit();
        session.ReplacePoints(ElementKey.Base(sample.TopBoundaryId), newPts);

        var newLib = session.Build();

        var boundaryId = EditingTestHelpers.FindFirstElementId(newLib, sample.Top, eid =>
        {
            if (newLib.Elements[eid].Kind != GdsElementKind.Boundary) return false;
            var p = newLib.Boundaries[newLib.Elements[eid].Index];
            return p is { Layer: 1, DataType: 10 };
        });

        Assert.That(boundaryId, Is.Not.EqualTo(-1));

        var er = newLib.Elements[boundaryId];
        var bp = newLib.Boundaries[er.Index];

        Assert.That(EditingTestHelpers.ReadVertices(newLib, bp.VertexOffset, bp.VertexCount), Is.EqualTo(newPts).AsCollection);

        var expectedBb = GdsBoundingBox.FromPoints(newPts);
        Assert.That(er.BoundingBox.HasValue, Is.True);
        Assert.That(er.BoundingBox!.Value.Min, Is.EqualTo(expectedBb.Min));
        Assert.That(er.BoundingBox!.Value.Max, Is.EqualTo(expectedBb.Max));
    }

    [Test]
    public void ReplacePoints_Path_UpdatesVertices_AndBoundingBoxExpandsByWidth()
    {
        var sample = EditingTestHelpers.CreateSampleLibrary();

        var newPts = new[]
        {
            new GdsPoint(10, 10),
            new GdsPoint(30, 10),
            new GdsPoint(30, 25)
        };

        using var session = sample.Lib.Edit();
        session.ReplacePoints(ElementKey.Base(sample.TopPathId), newPts);

        var newLib = session.Build();

        var pathId = EditingTestHelpers.FindFirstElementId(newLib, sample.Top, eid =>
        {
            if (newLib.Elements[eid].Kind != GdsElementKind.Path) return false;
            var p = newLib.Paths[newLib.Elements[eid].Index];
            return p is { Layer: 2, DataType: 20 } && p.Width == sample.TopPathWidth;
        });

        Assert.That(pathId, Is.Not.EqualTo(-1));

        var er = newLib.Elements[pathId];
        var pp = newLib.Paths[er.Index];

        Assert.That(EditingTestHelpers.ReadVertices(newLib, pp.VertexOffset, pp.VertexCount), Is.EqualTo(newPts).AsCollection);

        var expectedBb = EditingTestHelpers.ComputePathBoundingBox(newPts, sample.TopPathWidth);
        Assert.That(er.BoundingBox.HasValue, Is.True);
        Assert.That(er.BoundingBox!.Value.Min, Is.EqualTo(expectedBb.Min));
        Assert.That(er.BoundingBox!.Value.Max, Is.EqualTo(expectedBb.Max));
    }

    [Test]
    public void ReplacePoints_Box_UpdatesVertices()
    {
        var sample = EditingTestHelpers.CreateSampleLibrary();

        var newPts = new[]
        {
            new GdsPoint(200, 200),
            new GdsPoint(210, 200),
            new GdsPoint(210, 230),
            new GdsPoint(200, 230),
            new GdsPoint(200, 200)
        };

        using var session = sample.Lib.Edit();
        session.ReplacePoints(ElementKey.Base(sample.TopBoxId), newPts);

        var newLib = session.Build();

        var boxId = EditingTestHelpers.FindFirstElementId(newLib, sample.Top, eid =>
        {
            if (newLib.Elements[eid].Kind != GdsElementKind.Box) return false;
            var p = newLib.Boxes[newLib.Elements[eid].Index];
            return p is { Layer: 3, BoxType: 30 };
        });

        Assert.That(boxId, Is.Not.EqualTo(-1));

        var er = newLib.Elements[boxId];
        var xp = newLib.Boxes[er.Index];
        Assert.That(EditingTestHelpers.ReadVertices(newLib, xp.VertexOffset, GdsGlobals.BoxPointCount), Is.EqualTo(newPts).AsCollection);
    }

    [Test]
    public void ReplacePoints_Node_UpdatesVertices()
    {
        var sample = EditingTestHelpers.CreateSampleLibrary();

        var newPts = new[]
        {
            new GdsPoint(-1, -1),
            new GdsPoint(-2, -3),
            new GdsPoint(-5, -8)
        };

        using var session = sample.Lib.Edit();
        session.ReplacePoints(ElementKey.Base(sample.TopNodeId), newPts);

        var newLib = session.Build();

        var nodeId = EditingTestHelpers.FindFirstElementId(newLib, sample.Top, eid =>
        {
            if (newLib.Elements[eid].Kind != GdsElementKind.Node) return false;
            var p = newLib.Nodes[newLib.Elements[eid].Index];
            return p is { Layer: 4, NodeType: 40 };
        });

        Assert.That(nodeId, Is.Not.EqualTo(-1));

        var er = newLib.Elements[nodeId];
        var np = newLib.Nodes[er.Index];
        Assert.That(EditingTestHelpers.ReadVertices(newLib, np.VertexOffset, np.VertexCount), Is.EqualTo(newPts).AsCollection);
    }

    [Test]
    public void ReplacePoints_OnNonGeometry_Throws()
    {
        var sample = EditingTestHelpers.CreateSampleLibrary();
        using var session = sample.Lib.Edit();

        Assert.Throws<InvalidOperationException>(() =>
            session.ReplacePoints(ElementKey.Base(sample.TopTextId), new[] { new GdsPoint(0, 0) }));
    }
}
