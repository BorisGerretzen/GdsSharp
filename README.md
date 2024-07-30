# GdsSharp

[![NuGet](https://img.shields.io/nuget/v/GdsSharp.svg)](https://www.nuget.org/packages/GdsSharp/)\
A library for reading, editing, and writing [Calma GDSII](https://en.wikipedia.org/wiki/GDSII) files.

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

## Missing features

I have not implemented all features of the GDSii spec, some terminals
like [STRTYPE](https://boolean.klaasholwerda.nl/interface/bnf/gdsformat.html#rec_strtype) are not not released, and I am
not sure if they are used in files.
If you have a file and the library crashes because of this, let me know or open a PR!

Furthermore, I have also not
implemented [ATTRTABLE](https://boolean.klaasholwerda.nl/interface/bnf/gdsformat.html#rec_attrtable), none of the files
I currently have use it and I'm not sure how it is formatted exactly.
Again, if you have a file that uses it and want to contribute, let me know or open a PR!

## Helpers
Some helper functions are provided to draw curved shapes like circles and Bézier curves.

### Circle
```csharp
// Creates a circle at (0, 0) with radius 100. 
// An optional fourth parameter can be used to specify the number of points to use for the discretization of the circle.
var circleElement = GdsElement.CreateCircle(0, 0, 100);
```

### Bézier curve

The Bézier curve is created using a builder pattern. The curve is defined by a list of points, with the first and last points being the start and end points of the curve, and the intermediate points being control points.
Like the circle, an optional parameter can be used to specify the number of points to use for the discretization of the curve.
```csharp
var builder = new BezierBuilder()
    .AddPoint(0, 0)
    .AddPoint(100, 0)
    .AddPoint(100, 100);

// Creates a path from the control points with a width of 20.
var line = builder.BuildLine(20);

// Creates a polygon, closing the curve. 
var polygon = builder.BuildPolygon();
```


## Contributing

If you want to contribute, feel free to open a PR or issue.

## License

This project is licensed under the LGPL license, see the license file for the full text.

If this does not suit your needs, feel free to contact me and we can work something out.