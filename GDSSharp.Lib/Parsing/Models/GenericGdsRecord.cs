using GdsSharp.Lib.Models.Parsing;

namespace GdsSharp.Lib.Parsing.Models;

public class GenericGdsRecord<T> : IGdsSimpleRead
{
    public virtual T Value { get; set; } = default!;
}