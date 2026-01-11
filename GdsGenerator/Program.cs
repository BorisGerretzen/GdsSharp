using System.Numerics;
using GdsSharp.Lib;
using GdsSharp.Lib.Library;
using GdsSharp.Lib.Library.Builders;
using GdsSharp.Lib.Library.VertexStore;
using GdsSharp.Lib.Reading.Models;
using GdsSharp.Lib.Writing;

// Create a vertex store to hold all polygon/path vertices
var vertexStore = new MemoryVertexStore();

// Create the library builder
var builder = new GdsLibraryBuilder(vertexStore);

// Set library information
builder.Info = GdsLibraryInfo.Default with
{
    Name = "RandomObjects"
};

// Add a structure
builder.AddStructure(new GdsStructureInfo(
    Name: "MainStructure",
    CreationTime: DateTime.Now,
    ModificationTime: DateTime.Now
));

// Generate a Bézier curve as a polygon with width 200
var bezierBuilder1 = new BezierBuilder()
    .AddPoint(0, 0)
    .AddPoint(0, 1000)
    .AddPoint(1000, 1000)
    .AddPoint(1000, 0);
builder.AddBezierBoundary(layer: 1, dataType: 0, bezierBuilder1, width: 200);

// Generate another Bézier curve as a path
var bezierBuilder2 = new BezierBuilder()
    .AddPoint(-3000, 0)
    .AddPoint(-3000, 1000)
    .AddPoint(-2000, 1000)
    .AddPoint(-2000, 0);
builder.AddBezierPath(layer: 1, dataType: 0, bezierBuilder2, width: 200);

// Create a rectangle
builder.AddRectangle(layer: 1, dataType: 0, x: -3100, y: -1000, width: 4200, height: 1000);

// Create a circle
builder.AddCircle(layer: 1, dataType: 0, x: -1000, y: 744, radius: 350, numPoints: 128);

// Create a polygon by manually specifying the points
builder.AddBoundary(
    common: default,
    layer: 1,
    dataType: 0,
    points:
    [
        new GdsPoint(-1250, 0),
        new GdsPoint(-1250, 500),
        new GdsPoint(-1000, 250),
        new GdsPoint(-750, 500),
        new GdsPoint(-750, 0),
        new GdsPoint(-1250, 0)
    ]
);

// Use the path builder to create a complex path
var pathBuilder = new PathBuilder(
    initialWidth: 100f,
    initialPosition: new Vector2(-3100, -3300),
    initialHeading: Vector2.UnitX)
    
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
    .Straight(200, widthEnd: 250)
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
    .Straight(800);

// Add the path - it will be split into chunks of 200 vertices
builder.AddPath(layer: 1, dataType: 0, pathBuilder, maxVertices: 200);

// Build and Write to file
var library = builder.Build();
using var writeStream = File.OpenWrite("example.gds");
var writer = new GdsWriter(writeStream);
writer.Write(library, vertexStore);
