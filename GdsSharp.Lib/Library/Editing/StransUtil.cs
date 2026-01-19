using GdsSharp.Lib.Reading.Models;

namespace GdsSharp.Lib.Library.Editing;

internal static class StransUtil
{
    public static GdsStransInfo? Apply(GdsStransInfo? s, TransformOp op)
    {
        // GDS STRANS reflection bit is reflection about the X axis before rotation.
        var cur = s ?? GdsStransInfo.Default;
        var angle = cur.Angle ?? 0.0;

        switch (op)
        {
            case TransformOp.Translate:
                return s; // translation does not affect strans

            case TransformOp.Rotate90Clockwise:
                return cur with { Angle = angle + 90.0 };

            case TransformOp.Rotate90CounterClockwise:
                return cur with { Angle = angle - 90.0 };

            case TransformOp.MirrorX:
                // direct X-axis reflection
                return cur with { Reflection = !cur.Reflection };

            case TransformOp.MirrorY:
                // Y reflection can be represented as X reflection + 180 rotation
                return cur with { Reflection = !cur.Reflection, Angle = angle + 180.0 };

            default:
                return s;
        }
    }
}