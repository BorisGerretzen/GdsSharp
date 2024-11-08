using System.Numerics;
using GdsSharp.Lib.NonTerminals;

namespace GdsSharp.Lib;

public static class GdsExtensions
{
    /// <summary>
    ///     Rotates a vector by the given angle in radians.
    /// </summary>
    /// <param name="vec">Vector to rotate.</param>
    /// <param name="radians">Angle to rotate by.</param>
    /// <returns>Rotated vector.</returns>
    public static Vector2 Rotate(this Vector2 vec, float radians)
    {
        var ca = MathF.Cos(radians);
        var sa = MathF.Sin(radians);
        return new Vector2(ca * vec.X - sa * vec.Y, sa * vec.X + ca * vec.Y);
    }

    /// <summary>
    ///     Gets the angle of a vector in radians.
    /// </summary>
    /// <param name="vec">Vector.</param>
    /// <returns>Angle of the vector in radians.</returns>
    public static float Angle(this Vector2 vec)
    {
        return MathF.Atan2(vec.Y, vec.X);
    }
}