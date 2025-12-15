namespace GdsSharp.Lib.Parsing.Models;

public readonly record struct GdsStransInfo(bool Reflection, bool AbsoluteMagnification, bool AbsoluteAngle, double Magnification = 1.0, double Angle = 0);