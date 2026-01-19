using System.Buffers;

namespace GdsSharp.Lib;

public class PooledArray<T> : IDisposable
{
    internal PooledArray(T[] array, int length)
    {
        Array = array;
        Length = length;
    }

    public int Length { get; }
    public Span<T> Span => Array.AsSpan(0, Length);

    /// <summary>
    ///     If you really need the underlying array, you can access it here.
    ///     <list type="bullet">
    ///         <item>
    ///             Do not use this array after disposing the parent <see cref="PooledArray{T}" /> instance as this can
    ///             result in use-after-free bugs.
    ///         </item>
    ///         <item>
    ///             The array may be larger than <see cref="Length" /> items, make sure to only use the first
    ///             <see cref="Length" /> items.
    ///         </item>
    ///     </list>
    /// </summary>
    public T[] Array { get; }

    public void Dispose()
    {
        ArrayPool<T>.Shared.Return(Array);
    }
}