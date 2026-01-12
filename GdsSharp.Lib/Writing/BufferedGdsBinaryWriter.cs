namespace GdsSharp.Lib.Writing;

public sealed class BufferedGdsBinaryWriter(GdsWriteBuffer buffer)
{
    public int BytesWritten => buffer.Length;

    public void Write(byte v) => buffer.WriteByte(v);
    public void Write(ushort v) => buffer.WriteUshort(v);
    public void Write(short v) => buffer.WriteUshort(unchecked((ushort)v));
    public void Write(int v) => buffer.WriteInt(v);

    public void Write(double value)
    {
        var gdsDouble = new GdsDouble(value);
        gdsDouble.WriteTo(buffer.GetSpan(GdsDouble.Size));
    }

    public void Write(ReadOnlySpan<byte> bytes) => buffer.WriteBytes(bytes);

    public void Reset()
    {
        buffer.Clear();
    }
}