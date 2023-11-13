using System.Collections;

namespace GdsSharp.Lib.Test;

public static class Extensions
{
    public static TValue FirstOfType<TValue>(this IEnumerable enumerable)
    {
        return enumerable.OfType<TValue>().First();
    }
}