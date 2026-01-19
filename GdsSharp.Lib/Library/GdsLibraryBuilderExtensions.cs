using GdsSharp.Lib.Library.Builders;
using GdsSharp.Lib.Reading.Enum;
using GdsSharp.Lib.Reading.Models;

namespace GdsSharp.Lib.Library;

/// <summary>
///     Extension methods for GdsLibraryBuilder to support geometry builders.
/// </summary>
public static class GdsLibraryBuilderExtensions
{
    /// <summary>
    ///     Adds a Bézier curve as a boundary element (polygon).
    /// </summary>
    /// <param name="builder">The library builder.</param>
    /// <param name="layer">Layer number.</param>
    /// <param name="dataType">Data type.</param>
    /// <param name="bezierBuilder">Configured BezierBuilder with control points.</param>
    /// <param name="width">Width of the curve.</param>
    /// <param name="numVertices">Number of vertices for the curve.</param>
    /// <param name="common">Optional element common data.</param>
    /// <returns>Element ID of the created boundary.</returns>
    public static int AddBezierBoundary(
        this GdsLibraryBuilder builder,
        short layer,
        short dataType,
        BezierBuilder bezierBuilder,
        int width,
        int numVertices = 64,
        GdsElementCommon? common = null)
    {
        var points = bezierBuilder.BuildPolygonPoints(width, numVertices);
        return builder.AddBoundary(common, layer, dataType, points);
    }

    /// <summary>
    ///     Adds a Bézier curve as a path element.
    /// </summary>
    /// <param name="builder">The library builder.</param>
    /// <param name="layer">Layer number.</param>
    /// <param name="dataType">Data type.</param>
    /// <param name="bezierBuilder">Configured BezierBuilder with control points.</param>
    /// <param name="width">Width of the path.</param>
    /// <param name="numVertices">Number of vertices for the curve.</param>
    /// <param name="pathType">Optional path type.</param>
    /// <param name="common">Optional element common data.</param>
    /// <returns>Element ID of the created path.</returns>
    public static int AddBezierPath(
        this GdsLibraryBuilder builder,
        short layer,
        short dataType,
        BezierBuilder bezierBuilder,
        int width,
        int numVertices = 64,
        GdsPathType? pathType = null,
        GdsElementCommon? common = null)
    {
        var points = bezierBuilder.BuildLinePoints(numVertices);
        return builder.AddPath(common, layer, dataType, points, width, pathType);
    }

    /// <summary>
    ///     Adds a circle as a boundary element.
    /// </summary>
    /// <param name="builder">The library builder.</param>
    /// <param name="layer">Layer number.</param>
    /// <param name="dataType">Data type.</param>
    /// <param name="x">Center X coordinate.</param>
    /// <param name="y">Center Y coordinate.</param>
    /// <param name="radius">Radius of the circle.</param>
    /// <param name="numPoints">Number of points (must be a multiple of 8, default: 64).</param>
    /// <param name="common">Optional element common data.</param>
    /// <returns>Element ID of the created boundary.</returns>
    public static int AddCircle(
        this GdsLibraryBuilder builder,
        short layer,
        short dataType,
        int x,
        int y,
        int radius,
        int numPoints = 64,
        GdsElementCommon? common = null)
    {
        var points = CircleBuilder.CreateCirclePoints(x, y, radius, numPoints);
        return builder.AddBoundary(common, layer, dataType, points);
    }

    /// <summary>
    ///     Adds a rectangle as a boundary element.
    /// </summary>
    /// <param name="builder">The library builder.</param>
    /// <param name="layer">Layer number.</param>
    /// <param name="dataType">Data type.</param>
    /// <param name="x">Lower left X coordinate.</param>
    /// <param name="y">Lower left Y coordinate.</param>
    /// <param name="width">Width of the rectangle.</param>
    /// <param name="height">Height of the rectangle.</param>
    /// <param name="common">Optional element common data.</param>
    /// <returns>Element ID of the created boundary.</returns>
    public static int AddRectangle(
        this GdsLibraryBuilder builder,
        short layer,
        short dataType,
        int x,
        int y,
        int width,
        int height,
        GdsElementCommon? common = null)
    {
        var points = RectBuilder.CreateRectPoints(x, y, width, height);
        return builder.AddBoundary(common, layer, dataType, points);
    }

    /// <summary>
    ///     Adds a complex path as boundary elements.
    ///     The path may be split into multiple boundary elements if it exceeds the maximum vertex count.
    /// </summary>
    /// <param name="builder">The library builder.</param>
    /// <param name="layer">Layer number.</param>
    /// <param name="dataType">Data type.</param>
    /// <param name="pathBuilder">Configured PathBuilder.</param>
    /// <param name="maxVertices">Maximum vertices per boundary element (default: 200).</param>
    /// <param name="common">Optional element common data.</param>
    /// <returns>List of element IDs for the created boundaries.</returns>
    public static List<int> AddPath(
        this GdsLibraryBuilder builder,
        short layer,
        short dataType,
        PathBuilder pathBuilder,
        int maxVertices = 200,
        GdsElementCommon? common = null)
    {
        var elementIds = new List<int>();
        var elementCommon = common ?? default;

        foreach (var pointArray in pathBuilder.Build(maxVertices))
        {
            var elementId = builder.AddBoundary(elementCommon, layer, dataType, pointArray);
            elementIds.Add(elementId);
        }

        return elementIds;
    }
}