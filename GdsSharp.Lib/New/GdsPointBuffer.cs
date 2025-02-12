using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace GdsSharp.Lib.New;

public sealed class GdsPointDynamicBuffer : IDisposable
{
    private IntPtr _buffer;
    private int _capacity;
    private int _count;

    public GdsPointDynamicBuffer(int initialCapacity = 100_000)
    {
        _capacity = initialCapacity;
        _buffer = Marshal.AllocHGlobal(_capacity * Unsafe.SizeOf<GdsPoint>());
        _count = 0;
    }

    public unsafe Span<GdsPoint> GetSpan() => new(_buffer.ToPointer(), _count);

    public unsafe void AddPoints(ReadOnlySpan<GdsPoint> newPoints)
    {
        EnsureCapacity(_count + newPoints.Length);

        var bufferSpan = new Span<GdsPoint>(_buffer.ToPointer(), _capacity);
        newPoints.CopyTo(bufferSpan[_count..]);
        _count += newPoints.Length;
    }

    private void EnsureCapacity(int newCapacity)
    {
        unsafe
        {
            if (newCapacity <= _capacity) return;

            var newSize = _capacity * 2;
            var newBuffer = Marshal.AllocHGlobal(newSize * Unsafe.SizeOf<GdsPoint>());
            Buffer.MemoryCopy((void*)_buffer, (void*)newBuffer, newSize, _count * Unsafe.SizeOf<GdsPoint>());

            Marshal.FreeHGlobal(_buffer);
            _buffer = newBuffer;
            _capacity = newSize;
        }
    }

    public void Dispose()
    {
        Marshal.FreeHGlobal(_buffer);
    }
}
