namespace GdsSharp.Lib.Models;

public class GdsStrans
{
    /// <summary>
    ///     Magnification factor. Default is 1.0.
    /// </summary>
    public double Magnification { get; set; } = 1.0d;

    /// <summary>
    ///     Angle of rotation in degrees in the counterclockwise direction. Default is 0.0.
    /// </summary>
    public double Angle { get; set; } = 0.0d;
}