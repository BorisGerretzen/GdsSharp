namespace GdsSharp.Lib.Terminals.Abstractions;

public abstract class GenericGdsRecord<T> : IGdsSimpleRead, IGdsSimpleWrite
{
    public virtual T Value { get; set; } = default!;

    public abstract ushort Code { get; }

    public virtual ushort GetLength()
    {
        return Value switch
        {
            double => 8,
            ushort => 2,
            short => 2,
            uint => 4,
            int => 4,
            ulong => 8,
            long => 8,
            string s => (ushort)(s.Length % 2 == 0 ? s.Length : s.Length + 1),
            _ => throw new ArgumentOutOfRangeException(nameof(T), $"Cannot get size of type '{typeof(T)}'")
        };
    }
}