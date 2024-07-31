using GdsSharp.Lib.Builders;
using GdsSharp.Lib.NonTerminals;
using GdsSharp.Lib.NonTerminals.Elements;

var file = new GdsFile
{
    LibraryName = "RandomObjects",
    UserUnits = 1,
    PhysicalUnits = 1e-8,
    Version = 600
};

file.Structures.Add(new GdsStructure
{
    Name = "Example structure",
    Elements =
    [
        // Generate a line from a Bézier curve with width 20.
        // When using BuildPolygon the curve will be a GdsBoundaryElement.
        new BezierBuilder()
            .AddPoint(0, 0)
            .AddPoint(0, 1000)
            .AddPoint(1000, 1000)
            .AddPoint(1000, 0)
            .BuildPolygon(200),

        // When using BuildLine the curve will be a GdsPathElement.
        new BezierBuilder()
            .AddPoint(-3000, 0)
            .AddPoint(-3000, 1000)
            .AddPoint(-2000, 1000)
            .AddPoint(-2000, 0)
            .BuildLine(200),

        // Create a rectangle
        RectBuilder.CreateRect(-3100, -1000, 4200, 1000),

        // Create a circle 
        CircleBuilder.CreateCircle(-1000, 744, 350, 128),

        // Create a polygon by manually specifying the points
        new GdsElement
        {
            Element = new GdsBoundaryElement
            {
                Points =
                [
                    new GdsPoint(-1250, 0),
                    new GdsPoint(-1250, 500),
                    new GdsPoint(-1000, 250),
                    new GdsPoint(-750, 500),
                    new GdsPoint(-750, 0),
                    new GdsPoint(-1250, 0)
                ]
            }
        }
    ]
});

using var write = File.OpenWrite("example.gds");
file.WriteTo(write);