namespace GdsSharp.Lib.Parsing.Models;

public readonly record struct GdsStransInfo(bool Reflection, bool AbsoluteMagnification, bool AbsoluteAngle, double? Magnification = null, double? Angle = null);