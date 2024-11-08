using System.Numerics;
using GdsSharp.Lib;
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

var elements = new List<GdsElement>
{
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
    new()
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
            ],
            NumPoints = 6
        }
    }
};

// Use the path builder to create a path
// Returns an IEnumerable of GdsElement because the path may be split into multiple elements
elements.AddRange(
    new PathBuilder(
            100f,
            new Vector2(-3100, -3300),
            Vector2.UnitX)
        // Straight ahead for 2000 units
        .Straight(2000)

        // Bend 45 degrees to the left with a radius of 500 units
        .BendDeg(-45, 500)

        // Generate shape like <=>
        .Straight(100, widthEnd: 250)
        .Straight(100)
        .Straight(100, widthEnd: 100)

        // Some more bends
        .BendDeg(-45, 500)
        .Straight(100)
        .Straight(200, 250)
        .BendDeg(180, 300)
        .BendDeg(-180, 300)

        // Example of using a function to change the width
        .BendDeg(-180, 900, f => MathF.Cos(f * 50) * 100 + 150)

        // PathBuilder also supports Bézier curves
        .Bezier(b => b
                .AddPoint(0, 0)
                .AddPoint(0, 1000)
                .AddPoint(2000, 1000)
                .AddPoint(1000, 0),
            t => 250 - (250 - 50) * t)
        .Straight(800)

        // Build the path in sections of 200 vertices
        // This is the 'official' maximum number of vertices per element in GDSII
        // In practice, the number of vertices per element can be much higher
        .Build(200)
);

var structure = new GdsStructure
{
    Name = "Example structure",
    Elements = elements
};

file.Structures = [structure];
using var write = File.OpenWrite("example.gds");
file.WriteTo(write);