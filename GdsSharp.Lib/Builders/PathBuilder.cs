using System.Numerics;
using GdsSharp.Lib.NonTerminals;
using GdsSharp.Lib.NonTerminals.Elements;

namespace GdsSharp.Lib.Builders;

public class PathBuilder
{
    private readonly Vector2 _initialHeading;
    private readonly Vector2 _initialPosition;
    private readonly float _initialWidth;
    private readonly List<GdsPathSegment> _pathSegments = new();

    public PathBuilder(float initialWidth, Vector2? initialPosition = null, Vector2? initialHeading = null)
    {
        _initialWidth = initialWidth;
        _initialPosition = initialPosition ?? Vector2.Zero;
        _initialHeading = initialHeading ?? Vector2.UnitY;
    }

    /// <summary>
    ///     Adds a path segment to the path.
    /// </summary>
    /// <param name="path">Function that returns the coordinates of the path.</param>
    /// <param name="derivative">Function that returns derivative vectors of the path.</param>
    /// <param name="width">
    ///     Function that returns the width of the path. Can be null or return null, in this case the last
    ///     available width will be used.
    /// </param>
    /// <param name="vertices">Number of vertices used for the path. The final mesh will have two times this amount.</param>
    public PathBuilder AddPathSegment(Func<float, Vector2> path, Func<float, Vector2> derivative, Func<float, float?>? width = null, int vertices = 10)
    {
        _pathSegments.Add(new GdsPathSegment(path, derivative, width, vertices));
        return this;
    }

    /// <summary>
    ///     Adds a straight segment to the path.
    ///     If <see cref="width" /> is provided it takes precedence over <see cref="widthStart" /> and <see cref="widthEnd" />.
    ///     If only <see cref="widthEnd" /> is provided the width will be interpolated between the previous width and
    ///     <see cref="widthEnd" />.
    /// </summary>
    /// <param name="length">Length of the segment.</param>
    /// <param name="widthStart">(optional) Width at the start of the segment.</param>
    /// <param name="widthEnd">(optional) Width at the end of the segment.</param>
    /// <param name="width">(optional) Function that provides a width for t on the interval [0,1].</param>
    /// <param name="vertices">(optional) Number of vertices used for the path. Increase this if using <see cref="width" />.</param>
    public PathBuilder Straight(int length, float? widthStart = null, float? widthEnd = null, Func<float, float?>? width = null, int vertices = 2)
    {
        width ??= t => t < 0.5f ? widthStart : widthEnd;

        return AddPathSegment(
            t => new Vector2(0, t * length),
            _ => new Vector2(0, 1),
            width,
            vertices);
    }

    /// <summary>
    ///     Adds a bend to the path.
    /// </summary>
    /// <param name="angle">Angle of the bend in radians.</param>
    /// <param name="radius">Radius of the bend.</param>
    /// <param name="width">(optional) Function that provides a width for t on the interval [0,1].</param>
    /// <param name="vertices">(optional) Number of vertices used for the path.</param>
    public PathBuilder BendRad(float angle, int radius, Func<float, float?>? width = null, int vertices = 128)
    {
        var sign = Math.Sign(angle);
        angle = MathF.Abs(angle);
        var phase = sign == -1 ? MathF.PI : 0;

        return AddPathSegment(
            t => new Vector2(
                radius * (1 - MathF.Cos(t * angle)) * sign,
                radius * MathF.Sin(t * angle)
            ),
            t => new Vector2(
                angle * radius * MathF.Sin(t * angle + phase),
                angle * radius * MathF.Cos(t * angle)
            ),
            t => width?.Invoke(t),
            vertices);
    }

    /// <summary>
    ///     Adds a bend to the path.
    /// </summary>
    /// <param name="angle">Angle of the bend in degrees.</param>
    /// <param name="radius">Radius of the bend.</param>
    /// <param name="width">(optional) Function that provides a width for t on the interval [0,1].</param>
    /// <param name="vertices">(optional) Number of vertices used for the path.</param>
    public PathBuilder BendDeg(float angle, int radius, Func<float, float?>? width = null, int vertices = 128)
    {
        angle = MathF.PI * angle / 180;
        return BendRad(angle, radius, width, vertices);
    }

    /// <summary>
    ///     Adds a Bézier curve to the path.
    /// </summary>
    /// <param name="build">Bézier builder where you can add control points.</param>
    /// <param name="width">(optional) Function that provides a width for t on the interval [0,1].</param>
    /// <param name="vertices">(optional) Number of vertices used for the path.</param>
    public PathBuilder Bezier(Action<BezierBuilder> build, Func<float, float?>? width = null, int vertices = 128)
    {
        var builder = new BezierBuilder();
        build(builder);

        return AddPathSegment(
            t => builder.Evaluate(t),
            t => builder.EvaluateTangent(t), // It works like this without rotation for some reason
            t => width?.Invoke(t),
            vertices);
    }

    /// <summary>
    ///     Builds the path into a series of elements each having a maximum number of vertices.
    /// </summary>
    /// <param name="maxVertices">Maximum number of vertices per element.</param>
    /// <returns>Enumerable of <see cref="GdsBoundaryElement"/>.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="maxVertices"/> is less than 4.</exception>
    public IEnumerable<GdsElement> Build(int maxVertices = 200)
    {
        if (maxVertices < 4)
            throw new ArgumentException("maxVerticesPerElement must be at least 4 to form a valid polygon.", nameof(maxVertices));
        
        var ap = GetPathPoints();

        foreach (var points in ap.Chunk(maxVertices/2))
        {
            var allPoints = new GdsPoint[points.Length * 2];
            for(var i = 0; i < points.Length; i++)
            {
                allPoints[i] = new GdsPoint(points[i].Point + points[i].Width * points[i].Normal);
                allPoints[allPoints.Length - i - 1] = new GdsPoint(points[i].Point - points[i].Width * points[i].Normal);
            }
            
            
            yield return new GdsElement
            {
                Element = new GdsBoundaryElement
                {
                    Points = allPoints.ToList(),
                    NumPoints = allPoints.Length,
                }
            };
        }
    }
    /// <summary>
    ///     Generates a list of points for each segment in the path.
    /// </summary>
    /// <returns>List of points per segment.</returns>
    protected IEnumerable<GdsPathPoint> GetPathPoints()
    {
        var position = _initialPosition;
        var heading = _initialHeading;
        var currentWidth = _initialWidth;

        var unitYAngle = Vector2.UnitY.Angle();
        
        foreach (var segment in _pathSegments)
        {
            Vector2? lastPosition = null;
            Vector2? lastHeading = null;

            var rotationAngle = unitYAngle - heading.Angle();
            for (var i = 0; i <= segment.Vertices; i++)
            {
                var t = i / (float)segment.Vertices;

                // Get data from this point
                var point = segment.Path(t);
                var derivative = Vector2.Normalize(segment.Derivative(t));
                var width = segment.Width?.Invoke(t);

                // Correct the point and derivative for the heading
                point = point.Rotate(-rotationAngle);
                derivative = derivative.Rotate(-rotationAngle);

                lastHeading = derivative;
                lastPosition = point;
                currentWidth = width ?? currentWidth;

                var normal = new Vector2(-derivative.Y, derivative.X);
                yield return new GdsPathPoint(point + position, normal, currentWidth);
            }

            if (lastPosition.HasValue)
                position += lastPosition.Value;
            if (lastHeading.HasValue)
                heading = lastHeading.Value;
        }
    }

    /// <summary>
    ///     Represents a path segment.
    /// </summary>
    /// <param name="Path">Function that defines the path of the segment.</param>
    /// <param name="Derivative">Function that defines the derivative of the segment.</param>
    /// <param name="Width">Function that defines the width of the segment.</param>
    /// <param name="Vertices">Number of vertices of the segment.</param>
    protected record struct GdsPathSegment(Func<float, Vector2> Path, Func<float, Vector2> Derivative, Func<float, float?>? Width, int Vertices);

    /// <summary>
    ///     Represents a single point in the path.
    /// </summary>
    /// <param name="Point">The coordinates of the point.</param>
    /// <param name="Normal">The normal of the point.</param>
    /// <param name="Width">The width at the point.</param>
    protected record struct GdsPathPoint(Vector2 Point, Vector2 Normal, float Width);
}