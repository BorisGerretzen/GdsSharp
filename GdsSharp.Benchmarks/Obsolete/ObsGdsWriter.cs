using GdsSharp.Lib.Obsolete.Binary;
using GdsSharp.Lib.Obsolete.Terminals;
using GdsSharp.Lib.Obsolete.Terminals.Abstractions;

namespace GdsSharp.Lib.Obsolete;

public class ObsGdsWriter : GdsStreamOperator
{
    public static void Write(IEnumerable<IGdsRecord> records, Stream stream)
    {
        var writer = new GdsBinaryWriter(stream);
        foreach (var record in records)
        {
            if (record is not IGdsWriteableRecord writeableRecord)
                throw new InvalidOperationException($"Record {record.GetType().Name} is not writeable");

            var header = new GdsHeader
            {
                Code = record.Code,
                Length = (ushort)(record.GetLength() + GdsHeader.RecordSize)
            };
            ((IGdsSimpleWrite)header).Write(writer);
            writeableRecord.Write(writer);
        }
    }
}