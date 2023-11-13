using System.Reflection;

namespace GdsSharp.Lib.Parsing.Tokens;

public class GenericGdsRecord<T> : IGdsSimpleRead
{
    public virtual T Value { get; set; } = default!;
    public static bool CanRead => true;
}