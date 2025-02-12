using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using GdsSharp.Lib.Binary;

namespace GdsSharp.Lib.New;

public class NewGdsReader
{
    private readonly Stream _stream;

    public NewGdsReader(Stream stream)
    {
        _stream = stream;
    }

    public unsafe NewGdsHeader ReadHeader()
    {
        var size = Unsafe.SizeOf<NewGdsHeader>();
        Span<byte> buffer = stackalloc byte[size];
        if (_stream.Read(buffer) < size)
            throw new EndOfStreamException();
        
        var header = MemoryMarshal.Read<NewGdsHeader>(buffer);
        return header;
    }
    public unsafe NewGdsRecordXy ReadRecordXy(int count)
    {
        var size = Unsafe.SizeOf<NewGdsXyPoint>() * count;
        Span<byte> buffer = stackalloc byte[size];

        if (_stream.Read(buffer) < size)
            throw new EndOfStreamException();

        var coordinates = new NewGdsXyPoint[count];
        for (var i = 0; i < count; i++)
        {
            coordinates[i] = MemoryMarshal.Read<NewGdsXyPoint>(buffer[(i * Unsafe.SizeOf<NewGdsXyPoint>())..]);
        }

        return new NewGdsRecordXy(coordinates);
    }
    
}