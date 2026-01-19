using GdsSharp.Lib.Library;
using GdsSharp.Lib.Library.BoundingBox;
using GdsSharp.Lib.Library.Builder;
using GdsSharp.Lib.Library.VertexStore;
using GdsSharp.Lib.Reading;
using GdsSharp.Lib.Reading.Models;

namespace GdsSharp.Lib.Test.Editing;

internal static class EditingTestHelpers
{
    internal sealed class SampleLayout
    {
        public SampleLayout(
            GdsLibrary lib,
            CellId top,
            CellId child,
            int topBoundaryId,
            int topPathId,
            int topBoxId,
            int topNodeId,
            int topTextId,
            int topSRefId,
            int topARefId,
            int childBoundaryId,
            GdsPoint[] topBoundaryPoints,
            GdsPoint[] topPathPoints,
            int topPathWidth,
            GdsPoint[] topBoxPoints,
            GdsPoint[] topNodePoints,
            GdsPoint topTextOrigin,
            string topText,
            string refTargetName,
            GdsPoint srefOrigin,
            int arefRows,
            int arefColumns,
            GdsPoint arefRowVector,
            GdsPoint arefColVector,
            GdsPoint arefOrigin)
        {
            Lib = lib;
            Top = top;
            Child = child;

            TopBoundaryId = topBoundaryId;
            TopPathId = topPathId;
            TopBoxId = topBoxId;
            TopNodeId = topNodeId;
            TopTextId = topTextId;
            TopSRefId = topSRefId;
            TopARefId = topARefId;
            ChildBoundaryId = childBoundaryId;

            TopBoundaryPoints = topBoundaryPoints;
            TopPathPoints = topPathPoints;
            TopPathWidth = topPathWidth;
            TopBoxPoints = topBoxPoints;
            TopNodePoints = topNodePoints;

            TopTextOrigin = topTextOrigin;
            TopText = topText;

            RefTargetName = refTargetName;
            SRefOrigin = srefOrigin;

            ARefRows = arefRows;
            ARefColumns = arefColumns;
            ARefRowVector = arefRowVector;
            ARefColVector = arefColVector;
            ARefOrigin = arefOrigin;
        }

        public GdsLibrary Lib { get; }

        public CellId Top { get; }
        public CellId Child { get; }

        public int TopBoundaryId { get; }
        public int TopPathId { get; }
        public int TopBoxId { get; }
        public int TopNodeId { get; }
        public int TopTextId { get; }
        public int TopSRefId { get; }
        public int TopARefId { get; }
        public int ChildBoundaryId { get; }

        public GdsPoint[] TopBoundaryPoints { get; }
        public GdsPoint[] TopPathPoints { get; }
        public int TopPathWidth { get; }
        public GdsPoint[] TopBoxPoints { get; }
        public GdsPoint[] TopNodePoints { get; }

        public GdsPoint TopTextOrigin { get; }
        public string TopText { get; }

        public string RefTargetName { get; }
        public GdsPoint SRefOrigin { get; }

        public int ARefRows { get; }
        public int ARefColumns { get; }
        public GdsPoint ARefRowVector { get; }
        public GdsPoint ARefColVector { get; }
        public GdsPoint ARefOrigin { get; }
    }

    public static SampleLayout CreateSampleLibrary()
    {
        var store = new ChunkedVertexStore();
        var builder = new GdsLibraryBuilder(store)
        {
            Info = GdsLibraryInfo.Default with
            {
                Name = "TestLib",
                ModificationTime = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                AccessTime = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            }
        };

        var t = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        var top = builder.AddStructure(new GdsStructureInfo("TOP", t, t));

        var topBoundaryPoints = new[]
        {
            new GdsPoint(0, 0),
            new GdsPoint(10, 0),
            new GdsPoint(10, 10),
            new GdsPoint(0, 10),
            new GdsPoint(0, 0)
        };
        var topBoundaryId = builder.AddBoundary(null, layer: 1, dataType: 10, topBoundaryPoints);
        builder.AddElementProperty(topBoundaryId, attribute: 123, value: "base");

        var topPathPoints = new[]
        {
            new GdsPoint(0, 0),
            new GdsPoint(20, 0),
            new GdsPoint(20, 10)
        };
        var topPathWidth = 40;
        var topPathId = builder.AddPath(null, layer: 2, dataType: 20, topPathPoints, width: topPathWidth, pathType: null);

        var topBoxPoints = new[]
        {
            new GdsPoint(100, 100),
            new GdsPoint(120, 100),
            new GdsPoint(120, 115),
            new GdsPoint(100, 115),
            new GdsPoint(100, 100)
        };
        var topBoxId = builder.AddBox(null, layer: 3, dataType: 30, topBoxPoints);

        var topNodePoints = new[]
        {
            new GdsPoint(-5, -5),
            new GdsPoint(-10, -5)
        };
        var topNodeId = builder.AddNode(null, layer: 4, dataType: 40, topNodePoints);

        var topTextOrigin = new GdsPoint(1, 2);
        var topText = "HELLO";
        var topTextId = builder.AddText(null, layer: 5, textType: 50, presentation: null, pathType: null, width: null, strans: null,
            origin: topTextOrigin, text: topText);

        var targetName = "CHILD";
        var srefOrigin = new GdsPoint(100, 200);
        var topSRefId = builder.AddStructureReference(null, targetName, strans: null, origin: srefOrigin);

        var arefRows = 2;
        var arefCols = 3;
        var arefRow = new GdsPoint(0, 20);
        var arefCol = new GdsPoint(30, 0);
        var arefOrigin = new GdsPoint(50, 50);
        var topARefId = builder.AddArrayReference(null, targetName, strans: null, rows: arefRows, columns: arefCols,
            rowVector: arefRow, columnVector: arefCol, origin: arefOrigin);

        var child = builder.AddStructure(new GdsStructureInfo(targetName, t, t));

        var childBoundaryPoints = new[]
        {
            new GdsPoint(1000, 1000),
            new GdsPoint(1010, 1000),
            new GdsPoint(1010, 1010),
            new GdsPoint(1000, 1010),
            new GdsPoint(1000, 1000)
        };
        var childBoundaryId = builder.AddBoundary(null, layer: 11, dataType: 110, childBoundaryPoints);

        var lib = builder.Build();
        return new SampleLayout(
            lib,
            top,
            child,
            topBoundaryId,
            topPathId,
            topBoxId,
            topNodeId,
            topTextId,
            topSRefId,
            topARefId,
            childBoundaryId,
            topBoundaryPoints,
            topPathPoints,
            topPathWidth,
            topBoxPoints,
            topNodePoints,
            topTextOrigin,
            topText,
            targetName,
            srefOrigin,
            arefRows,
            arefCols,
            arefRow,
            arefCol,
            arefOrigin);
    }

    public static IReadOnlyList<int> GetElementIds(GdsLibrary lib, CellId structureId)
    {
        var s = lib.Structures[structureId.Id];
        return Enumerable.Range(s.ElementStartIndex, s.ElementCount).ToArray();
    }

    public static int GetElementIdByOrdinal(GdsLibrary lib, CellId structureId, int ordinal)
    {
        var s = lib.Structures[structureId.Id];
        if (ordinal < 0 || ordinal >= s.ElementCount) throw new ArgumentOutOfRangeException(nameof(ordinal));
        return s.ElementStartIndex + ordinal;
    }

    public static int FindFirstElementId(GdsLibrary lib, CellId structureId, Func<int, bool> predicate)
    {
        foreach (var eid in GetElementIds(lib, structureId))
        {
            if (predicate(eid)) return eid;
        }
        return -1;
    }

    public static GdsPoint[] ReadVertices(GdsLibrary lib, long offset, int count)
    {
        var pts = new GdsPoint[count];
        lib.VertexStore.Read(offset, pts);
        return pts;
    }

    public static GdsPoint[] ReadGeometryPoints(GdsLibrary lib, int elementId)
    {
        var er = lib.Elements[elementId];
        return er.Kind switch
        {
            GdsElementKind.Boundary => ReadVertices(lib, lib.Boundaries[er.Index].VertexOffset, lib.Boundaries[er.Index].VertexCount),
            GdsElementKind.Path => ReadVertices(lib, lib.Paths[er.Index].VertexOffset, lib.Paths[er.Index].VertexCount),
            GdsElementKind.Box => ReadVertices(lib, lib.Boxes[er.Index].VertexOffset, GdsGlobals.BoxPointCount),
            GdsElementKind.Node => ReadVertices(lib, lib.Nodes[er.Index].VertexOffset, lib.Nodes[er.Index].VertexCount),
            _ => throw new InvalidOperationException("Element does not have vertices")
        };
    }

    public static IReadOnlyList<PropertyRecord> GetElementProperties(GdsLibrary lib, int elementId)
        => lib.Properties.Where(p => p.ElementId == elementId).ToArray();

    public static GdsBoundingBox ComputePathBoundingBox(ReadOnlySpan<GdsPoint> points, int? width)
    {
        var half = (width ?? 0) / 2;
        var bb = GdsBoundingBox.FromPoints(points);
        return new GdsBoundingBox(
            new GdsPoint(bb.Min.X - half, bb.Min.Y - half),
            new GdsPoint(bb.Max.X + half, bb.Max.Y + half));
    }
}
