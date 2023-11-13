namespace GdsSharp.Lib.Parsing;

public class GenericGdsRecord<T> : IGdsSimpleRead
{
    public virtual T Value { get; set; } = default!;
}