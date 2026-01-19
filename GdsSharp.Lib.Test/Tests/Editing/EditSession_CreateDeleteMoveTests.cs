using GdsSharp.Lib.Library.Editing;
using GdsSharp.Lib.Reading;

namespace GdsSharp.Lib.Test.Editing;

[TestFixture]
public class EditSessionCreateDeleteMoveTests
{
    [Test]
    public void Create_Boundary_Path_Box_Node_Text_SRef_ARef_AppearAfterReconcile_InOrder()
    {
        var sample = EditingTestHelpers.CreateSampleLibrary();
        var baseTopCount = sample.Lib.Structures[sample.Top.Id].ElementCount;

        using var session = sample.Lib.Edit();

        var bPts = new[]
        {
            new GdsPoint(200, 0),
            new GdsPoint(210, 0),
            new GdsPoint(210, 10),
            new GdsPoint(200, 10),
            new GdsPoint(200, 0)
        };
        session.CreateBoundary(sample.Top, layer: 9, dataType: 90, points: bPts);

        var pPts = new[]
        {
            new GdsPoint(0, 200),
            new GdsPoint(10, 220),
            new GdsPoint(30, 230)
        };
        session.CreatePath(sample.Top, layer: 8, dataType: 80, points: pPts, width: 12, pathType: null);

        var xPts = new[]
        {
            new GdsPoint(300, 300),
            new GdsPoint(310, 300),
            new GdsPoint(310, 320),
            new GdsPoint(300, 320),
            new GdsPoint(300, 300)
        };
        session.CreateBox(sample.Top, layer: 7, boxType: 70, points: xPts);

        var nPts = new[]
        {
            new GdsPoint(-100, 50),
            new GdsPoint(-110, 55)
        };
        session.CreateNode(sample.Top, layer: 6, nodeType: 60, points: nPts);

        session.CreateText(sample.Top, layer: 55, textType: 5, origin: new GdsPoint(9, 9), text: "NEW");
        session.CreateStructureReferences(sample.Top, targetName: "CHILD", origin: new GdsPoint(7, 8));
        session.CreateArrayReferences(sample.Top, targetName: "CHILD", rows: 2, columns: 2, rowVector: new GdsPoint(0, 10), colVector: new GdsPoint(10, 0), origin: new GdsPoint(1, 1));

        var newLib = session.Build();

        Assert.That(newLib.Structures[sample.Top.Id].ElementCount, Is.EqualTo(baseTopCount + 7));

        // The 7 created elements are appended in the order they were created.
        var eidBoundary = EditingTestHelpers.GetElementIdByOrdinal(newLib, sample.Top, baseTopCount + 0);
        var eidPath = EditingTestHelpers.GetElementIdByOrdinal(newLib, sample.Top, baseTopCount + 1);
        var eidBox = EditingTestHelpers.GetElementIdByOrdinal(newLib, sample.Top, baseTopCount + 2);
        var eidNode = EditingTestHelpers.GetElementIdByOrdinal(newLib, sample.Top, baseTopCount + 3);
        var eidText = EditingTestHelpers.GetElementIdByOrdinal(newLib, sample.Top, baseTopCount + 4);
        var eidSRef = EditingTestHelpers.GetElementIdByOrdinal(newLib, sample.Top, baseTopCount + 5);
        var eidARef = EditingTestHelpers.GetElementIdByOrdinal(newLib, sample.Top, baseTopCount + 6);

        Assert.That(newLib.Elements[eidBoundary].Kind, Is.EqualTo(GdsElementKind.Boundary));
        var bp = newLib.Boundaries[newLib.Elements[eidBoundary].Index];
        Assert.That(bp.Layer, Is.EqualTo((short)9));
        Assert.That(bp.DataType, Is.EqualTo((short)90));
        Assert.That(EditingTestHelpers.ReadVertices(newLib, bp.VertexOffset, bp.VertexCount), Is.EqualTo(bPts).AsCollection);

        Assert.That(newLib.Elements[eidPath].Kind, Is.EqualTo(GdsElementKind.Path));
        var pp = newLib.Paths[newLib.Elements[eidPath].Index];
        Assert.That(pp.Layer, Is.EqualTo((short)8));
        Assert.That(pp.DataType, Is.EqualTo((short)80));
        Assert.That(pp.Width, Is.EqualTo(12));
        Assert.That(EditingTestHelpers.ReadVertices(newLib, pp.VertexOffset, pp.VertexCount), Is.EqualTo(pPts).AsCollection);

        Assert.That(newLib.Elements[eidBox].Kind, Is.EqualTo(GdsElementKind.Box));
        var xp = newLib.Boxes[newLib.Elements[eidBox].Index];
        Assert.That(xp.Layer, Is.EqualTo((short)7));
        Assert.That(xp.BoxType, Is.EqualTo((short)70));
        Assert.That(EditingTestHelpers.ReadVertices(newLib, xp.VertexOffset, GdsGlobals.BoxPointCount), Is.EqualTo(xPts).AsCollection);

        Assert.That(newLib.Elements[eidNode].Kind, Is.EqualTo(GdsElementKind.Node));
        var np = newLib.Nodes[newLib.Elements[eidNode].Index];
        Assert.That(np.Layer, Is.EqualTo((short)6));
        Assert.That(np.NodeType, Is.EqualTo((short)60));
        Assert.That(EditingTestHelpers.ReadVertices(newLib, np.VertexOffset, np.VertexCount), Is.EqualTo(nPts).AsCollection);

        Assert.That(newLib.Elements[eidText].Kind, Is.EqualTo(GdsElementKind.Text));
        var tp = newLib.Texts[newLib.Elements[eidText].Index];
        Assert.That(tp.Layer, Is.EqualTo((short)55));
        Assert.That(tp.TextType, Is.EqualTo((short)5));
        Assert.That(tp.Origin, Is.EqualTo(new GdsPoint(9, 9)));
        Assert.That(tp.Text, Is.EqualTo("NEW"));

        Assert.That(newLib.Elements[eidSRef].Kind, Is.EqualTo(GdsElementKind.StructureReference));
        var sp = newLib.StructureReferences[newLib.Elements[eidSRef].Index];
        Assert.That(sp.TargetName, Is.EqualTo("CHILD"));
        Assert.That(sp.Origin, Is.EqualTo(new GdsPoint(7, 8)));

        Assert.That(newLib.Elements[eidARef].Kind, Is.EqualTo(GdsElementKind.ArrayReference));
        var ap = newLib.ArrayReferences[newLib.Elements[eidARef].Index];
        Assert.That(ap.TargetName, Is.EqualTo("CHILD"));
        Assert.That(ap.Rows, Is.EqualTo(2));
        Assert.That(ap.Columns, Is.EqualTo(2));
        Assert.That(ap.RowVector, Is.EqualTo(new GdsPoint(0, 10)));
        Assert.That(ap.ColumnVector, Is.EqualTo(new GdsPoint(10, 0)));
        Assert.That(ap.Origin, Is.EqualTo(new GdsPoint(1, 1)));
    }

    [Test]
    public void Delete_BaseElement_RemovesFromStructureSequence()
    {
        var sample = EditingTestHelpers.CreateSampleLibrary();
        var baseTopCount = sample.Lib.Structures[sample.Top.Id].ElementCount;

        using var session = sample.Lib.Edit();
        session.DeleteElement(sample.Top, ElementKey.Base(sample.TopBoundaryId));

        var newLib = session.Build();
        Assert.That(newLib.Structures[sample.Top.Id].ElementCount, Is.EqualTo(baseTopCount - 1));

        // Boundary was first in TOP; after deleting it, the first element should be the path.
        var firstId = EditingTestHelpers.GetElementIdByOrdinal(newLib, sample.Top, 0);
        Assert.That(newLib.Elements[firstId].Kind, Is.EqualTo(GdsElementKind.Path));
    }

    [Test]
    public void Delete_NewElement_CancelsCreation()
    {
        var sample = EditingTestHelpers.CreateSampleLibrary();
        var baseTopCount = sample.Lib.Structures[sample.Top.Id].ElementCount;

        using var session = sample.Lib.Edit();

        var pts = new[]
        {
            new GdsPoint(1, 1),
            new GdsPoint(2, 1),
            new GdsPoint(2, 2),
            new GdsPoint(1, 2),
            new GdsPoint(1, 1)
        };

        var created = session.CreateBoundary(sample.Top, layer: 99, dataType: 99, points: pts);
        session.DeleteElement(sample.Top, created);

        var newLib = session.Build();
        Assert.That(newLib.Structures[sample.Top.Id].ElementCount, Is.EqualTo(baseTopCount));

        var found = EditingTestHelpers.FindFirstElementId(newLib, sample.Top, eid =>
        {
            if (newLib.Elements[eid].Kind != GdsElementKind.Boundary) return false;
            var p = newLib.Boundaries[newLib.Elements[eid].Index];
            return p is { Layer: 99, DataType: 99 };
        });
        Assert.That(found, Is.EqualTo(-1));
    }

    [Test]
    public void Move_BasePath_ToChild_UpdatesStructureMembership_AndGeometry()
    {
        var sample = EditingTestHelpers.CreateSampleLibrary();
        var baseTopCount = sample.Lib.Structures[sample.Top.Id].ElementCount;
        var baseChildCount = sample.Lib.Structures[sample.Child.Id].ElementCount;

        using var session = sample.Lib.Edit();
        session.MoveElement(sample.Top, sample.Child, ElementKey.Base(sample.TopPathId));

        var newLib = session.Build();
        Assert.That(newLib.Structures[sample.Top.Id].ElementCount, Is.EqualTo(baseTopCount - 1));
        Assert.That(newLib.Structures[sample.Child.Id].ElementCount, Is.EqualTo(baseChildCount + 1));

        var movedId = EditingTestHelpers.FindFirstElementId(newLib, sample.Child, eid =>
        {
            if (newLib.Elements[eid].Kind != GdsElementKind.Path) return false;
            var p = newLib.Paths[newLib.Elements[eid].Index];
            return p is { Layer: 2, DataType: 20 } && p.Width == sample.TopPathWidth;
        });
        Assert.That(movedId, Is.Not.EqualTo(-1));
        Assert.That(EditingTestHelpers.ReadGeometryPoints(newLib, movedId), Is.EqualTo(sample.TopPathPoints).AsCollection);
    }

    [Test]
    public void Move_BaseSRef_ToChild_ReconciledParentMatchesDestination()
    {
        var sample = EditingTestHelpers.CreateSampleLibrary();

        using var session = sample.Lib.Edit();
        session.MoveElement(sample.Top, sample.Child, ElementKey.Base(sample.TopSRefId));

        var newLib = session.Build();

        var srefId = EditingTestHelpers.FindFirstElementId(newLib, sample.Child, eid =>
        {
            if (newLib.Elements[eid].Kind != GdsElementKind.StructureReference) return false;
            var p = newLib.StructureReferences[newLib.Elements[eid].Index];
            return p.TargetName == sample.RefTargetName;
        });

        Assert.That(srefId, Is.Not.EqualTo(-1));
        var sref = newLib.StructureReferences[newLib.Elements[srefId].Index];
        Assert.That(sref.Parent.Id, Is.EqualTo(sample.Child.Id));
    }
}
