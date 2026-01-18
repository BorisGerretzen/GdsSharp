using GdsSharp.Lib.Library;
using GdsSharp.Lib.Library.VertexStore;
using GdsSharp.Lib.Reading.Enum;
using GdsSharp.Lib.Reading.Models;

namespace GdsSharp.Lib.Test.Helpers;

internal static class TestLibraryFactory
{
    internal sealed record Expected(
        int ChildStructureIndex,
        int TopStructureIndex,
        int EmptyStructureIndex,
        int ChildBoundaryElementIndex,
        int TopBoundary1ElementIndex,
        int TopPathElementIndex,
        int TopBoxElementIndex,
        int TopNodeElementIndex,
        int TopTextElementIndex,
        int TopSrefElementIndex,
        int TopArefElementIndex,
        int TopBoundary2ElementIndex,
        GdsPoint[] ChildBoundaryPoints,
        GdsPoint[] TopBoundary1Points,
        GdsPoint[] TopBoundary2Points,
        GdsPoint[] TopPathPoints,
        GdsPoint[] TopBoxPoints,
        GdsPoint[] TopNodePoints,
        GdsPoint TopTextOrigin,
        string TopText,
        string ChildName,
        string TopName,
        string EmptyName,
        string LibraryName,
        int TopPathWidth,
        GdsPathType TopPathType,
        short TopBoundaryLayer,
        short TopBoundaryDataType,
        short TopBoxLayer,
        short TopBoxDataType,
        short TopNodeLayer,
        short TopNodeDataType,
        short TopTextLayer,
        short TopTextType,
        string RefTargetName,
        int ArefRows,
        int ArefCols,
        GdsPoint ArefRowVector,
        GdsPoint ArefColVector,
        GdsPoint RefOrigin);

    internal static (GdsLibrary Library, Expected Expected) Create()
    {
        var store = new ChunkedVertexStore();
        var builder = new GdsLibraryBuilder(store)
        {
            Info = GdsLibraryInfo.Default with
            {
                Name = "TestLib",
                // Fix to deterministic times so tests don't depend on wall clock
                ModificationTime = new DateTime(2020, 1, 1, 12, 0, 0, DateTimeKind.Utc),
                AccessTime = new DateTime(2020, 1, 1, 12, 0, 0, DateTimeKind.Utc)
            }
        };

        var childName = "CHILD";
        var topName = "TOP";
        var emptyName = "EMPTY";

        // CHILD
        var childIdx = builder.AddStructure(new GdsStructureInfo(childName,
            new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc)));

        var childBoundaryPoints = new[]
        {
            new GdsPoint(0, 0),
            new GdsPoint(10, 0),
            new GdsPoint(10, 10),
            new GdsPoint(0, 10),
            new GdsPoint(0, 0),
        };

        var childBoundaryElementIndex = builder.AddBoundary(common: null, layer: 1, dataType: 0, points: childBoundaryPoints);

        // TOP
        var topIdx = builder.AddStructure(new GdsStructureInfo(topName,
            new DateTime(2020, 1, 2, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2020, 1, 2, 0, 0, 0, DateTimeKind.Utc)));

        const short topBoundaryLayer = 5;
        const short topBoundaryDataType = 11;
        var topBoundary1Points = new[]
        {
            new GdsPoint(100, 100),
            new GdsPoint(120, 100),
            new GdsPoint(120, 130),
            new GdsPoint(100, 130),
            new GdsPoint(100, 100),
        };
        var topBoundary1ElementIndex = builder.AddBoundary(null, topBoundaryLayer, topBoundaryDataType, topBoundary1Points);

        var topPathPoints = new[]
        {
            new GdsPoint(200, 200),
            new GdsPoint(240, 200),
            new GdsPoint(240, 260),
            new GdsPoint(260, 300),
        };
        const int topPathWidth = 40;
        const GdsPathType topPathType = GdsPathType.Square;
        var topPathElementIndex = builder.AddPath(null, layer: 6, dataType: 12, points: topPathPoints, width: topPathWidth, pathType: topPathType);

        const short topBoxLayer = 7;
        const short topBoxDataType = 13;
        var topBoxPoints = new[]
        {
            new GdsPoint(300, 300),
            new GdsPoint(340, 300),
            new GdsPoint(340, 360),
            new GdsPoint(300, 360),
            new GdsPoint(300, 300),
        };
        var topBoxElementIndex = builder.AddBox(null, topBoxLayer, topBoxDataType, topBoxPoints);

        const short topNodeLayer = 8;
        const short topNodeDataType = 14;
        var topNodePoints = new[]
        {
            new GdsPoint(400, 400),
            new GdsPoint(420, 410),
            new GdsPoint(430, 430),
        };
        var topNodeElementIndex = builder.AddNode(null, topNodeLayer, topNodeDataType, topNodePoints);

        const short topTextLayer = 9;
        const short topTextType = 2;
        var topTextOrigin = new GdsPoint(500, 500);
        var topText = "hello";
        var topTextElementIndex = builder.AddText(null, topTextLayer, topTextType,
            presentation: null, pathType: null, width: null, strans: null,
            origin: topTextOrigin,
            text: topText);

        // References
        var refOrigin = new GdsPoint(1000, 1000);
        var topSrefElementIndex = builder.AddStructureReference(null, targetName: childName, strans: null, origin: refOrigin);

        var arefRows = 2;
        var arefCols = 3;
        var arefRowVector = new GdsPoint(1100, 1000);
        var arefColVector = new GdsPoint(1000, 1200);
        var topArefElementIndex = builder.AddArrayReference(null, targetName: childName, strans: null,
            rows: arefRows, columns: arefCols,
            rowVector: arefRowVector, columnVector: arefColVector,
            origin: refOrigin);

        var topBoundary2Points = new[]
        {
            new GdsPoint(-50, -50),
            new GdsPoint(-10, -50),
            new GdsPoint(-10, -10),
            new GdsPoint(-50, -10),
            new GdsPoint(-50, -50),
        };
        var topBoundary2ElementIndex = builder.AddBoundary(null, topBoundaryLayer, dataType: (short)(topBoundaryDataType + 1), points: topBoundary2Points);

        // EMPTY
        var emptyIdx = builder.AddStructure(new GdsStructureInfo(emptyName,
            new DateTime(2020, 1, 3, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2020, 1, 3, 0, 0, 0, DateTimeKind.Utc)));

        var lib = builder.Build();

        var expected = new Expected(
            ChildStructureIndex: childIdx.Id,
            TopStructureIndex: topIdx.Id,
            EmptyStructureIndex: emptyIdx.Id,
            ChildBoundaryElementIndex: childBoundaryElementIndex,
            TopBoundary1ElementIndex: topBoundary1ElementIndex,
            TopPathElementIndex: topPathElementIndex,
            TopBoxElementIndex: topBoxElementIndex,
            TopNodeElementIndex: topNodeElementIndex,
            TopTextElementIndex: topTextElementIndex,
            TopSrefElementIndex: topSrefElementIndex,
            TopArefElementIndex: topArefElementIndex,
            TopBoundary2ElementIndex: topBoundary2ElementIndex,
            ChildBoundaryPoints: childBoundaryPoints,
            TopBoundary1Points: topBoundary1Points,
            TopBoundary2Points: topBoundary2Points,
            TopPathPoints: topPathPoints,
            TopBoxPoints: topBoxPoints,
            TopNodePoints: topNodePoints,
            TopTextOrigin: topTextOrigin,
            TopText: topText,
            ChildName: childName,
            TopName: topName,
            EmptyName: emptyName,
            LibraryName: builder.Info!.Value.Name,
            TopPathWidth: topPathWidth,
            TopPathType: topPathType,
            TopBoundaryLayer: topBoundaryLayer,
            TopBoundaryDataType: topBoundaryDataType,
            TopBoxLayer: topBoxLayer,
            TopBoxDataType: topBoxDataType,
            TopNodeLayer: topNodeLayer,
            TopNodeDataType: topNodeDataType,
            TopTextLayer: topTextLayer,
            TopTextType: topTextType,
            RefTargetName: childName,
            ArefRows: arefRows,
            ArefCols: arefCols,
            ArefRowVector: arefRowVector,
            ArefColVector: arefColVector,
            RefOrigin: refOrigin);

        return (lib, expected);
    }
}
