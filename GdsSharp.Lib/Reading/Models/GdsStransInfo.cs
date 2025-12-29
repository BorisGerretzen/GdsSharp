namespace GdsSharp.Lib.Reading.Models;

public readonly record struct GdsStransInfo(bool Reflection, bool AbsoluteMagnification, bool AbsoluteAngle, double? Magnification = null, double? Angle = null)
{
    public static readonly GdsStransInfo Default = new(false, false, false, 1, 0);
}