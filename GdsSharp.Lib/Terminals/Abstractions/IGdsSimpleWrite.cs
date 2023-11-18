using System.Reflection;

namespace GdsSharp.Lib.Terminals.Abstractions;

public interface IGdsSimpleWrite : IGdsWriteableRecord
{
    void IGdsWriteableRecord.Write(GdsBinaryWriter writer)
    {
        foreach (var property in GetType()
                     .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.SetProperty))
        {
            if (property.GetSetMethod() == null) continue;

            switch (property.GetValue(this))
            {
                case double d:
                    writer.Write(d);
                    break;
                case ushort u:
                    writer.Write(u);
                    break;
                case short s:
                    writer.Write(s);
                    break;
                case uint u:
                    writer.Write(u);
                    break;
                case int i:
                    writer.Write(i);
                    break;
                case ulong u:
                    writer.Write(u);
                    break;
                case long l:
                    writer.Write(l);
                    break;
                case string s:
                    writer.Write(s);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(property), property,
                        $"Cannot write type '{property.PropertyType}'");
            }
        }
    }
}