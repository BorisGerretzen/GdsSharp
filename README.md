# GdsSharp

[![NuGet](https://img.shields.io/nuget/v/GdsSharp.svg)](https://www.nuget.org/packages/GdsSharp/)\
A library for reading, editing, and writing [Calma GDSII](https://en.wikipedia.org/wiki/GDSII) files.

Some helpers are also provided for drawing shapes like circles and Bézier curves.
Additionally, a path builder is included to create parameterized paths with multiple segments.

## Usage

### Reading a GDSii file

```csharp
using var fileStream = File.OpenRead("file.gds");
var file = GdsFile.From(fileStream);
```

### Writing a GDSii file

```csharp
using var fileStream = File.OpenWrite("file.gds");
file.WriteTo(fileStream);
```

### Editing example

An example of how to create a GDS file from scratch and creating some shapes using the helpers can be found in
the [example project](https://github.com/BorisGerretzen/GdsSharp/blob/master/GdsGenerator/Program.cs).
This is the created GDS file:
![image](https://github.com/user-attachments/assets/6629195b-64e3-4778-87e7-d8cae87826d7)


## Helpers
Some helper functions are provided to draw curved shapes like circles and Bézier curves.

### Circle
```csharp
// Creates a circle at (0, 0) with radius 100. 
// An optional fourth parameter can be used to specify the number of points to use for the discretization of the circle.
var elemCircle = CircleBuilder.CreateCircle(x: 0, y: 0, radius: 100, numPoints: 128),
```

### Rect

```csharp
// Creates a rectangle at (0, 0) with width 100 and height 200.
var elemRect = RectBuilder.CreateRect(x: 0, y: 0, width: 100, height: 200);
```

### Bézier curve

The Bézier curve is created using a builder pattern. The curve is defined by a list of control points, the library supports a maximum of 16 control points per curve.
Like the circle, an optional parameter can be used to specify the number of points to use for the discretization of the curve.
```csharp
// Generate a line from a Bézier curve with width 200.
// When using BuildPolygon the curve will be a GdsBoundaryElement.
var elemPoly = new BezierBuilder()
    .AddPoint(x: 0, y: 0)
    .AddPoint(0, 1000)
    .AddPoint(1000, 1000)
    .AddPoint(1000, 0)
    .BuildPolygon(width: 200, numVertices: 128);

// When using BuildLine the curve will be a GdsPathElement.
// BuildPolygon is recommended because it produces a smaller error in the curve.
var elemLine = new BezierBuilder()
    .AddPoint(x: 0, y: 0)
    .AddPoint(0, 1000)
    .AddPoint(1000, 1000)
    .AddPoint(1000, 0)
    .BuildLine(width: 200, numVertices: 128);
```

### Path builder

GdsSharp also includes a path builder that can be used to create paths with multiple segments.
The output of this code can be seen on the bottom part of the picture above.

```csharp
// Use the path builder to create a path
new PathBuilder(
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
    .Straight(200, widthStart: 250)
    .BendDeg(180, 300)
    .BendDeg(-180, 300)
    
    // Example of using a function to change the width
    .BendDeg(-180, 900, f => MathF.Cos(f * 50) * 100 + 150)
    
    // PathBuilder also supports Bézier curves
    .Bezier(b => b
        .AddPoint(0, 0)
        .AddPoint(0,1000)
        .AddPoint(2000, 1000)
        .AddPoint(1000, 0),
        width: t => 250 - (250-50) * t)
    .Straight(800)
    .Build();
```

## Missing features

I have not implemented all features of the GDSii spec, some terminals
like [STRTYPE](https://boolean.klaasholwerda.nl/interface/bnf/gdsformat.html#rec_strtype) are not not released, and I am
not sure if they are used in files.
If you have a file and the library crashes because of this, let me know or open a PR!

Furthermore, I have also not
implemented [ATTRTABLE](https://boolean.klaasholwerda.nl/interface/bnf/gdsformat.html#rec_attrtable), none of the files
I currently have use it and I'm not sure how it is formatted exactly.
Again, if you have a file that uses it and want to contribute, let me know or open a PR!

## Contributing

If you want to contribute, feel free to open a PR or issue.

## License

This project is licensed under the LGPL license, see the license file for the full text.

If this does not suit your needs, feel free to contact me and we can work something out.
