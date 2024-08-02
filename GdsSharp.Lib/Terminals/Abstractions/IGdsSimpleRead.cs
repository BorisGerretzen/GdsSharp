using System.Reflection;
using GdsSharp.Lib.Binary;

namespace GdsSharp.Lib.Terminals.Abstractions;

public interface IGdsSimpleRead : IGdsReadableRecord
{
    void IGdsReadableRecord.Read(GdsBinaryReader reader, GdsHeader header)
    {
        foreach (var property in GetType()
                     .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.SetProperty))
        {
            if (property.GetSetMethod() == null) continue;

            var propertyType = property.PropertyType;

            object valueToSet = propertyType switch
            {
                not null when propertyType == typeof(ushort) => reader.ReadUInt16(),
                not null when propertyType == typeof(uint) => reader.ReadUInt32(),
                not null when propertyType == typeof(short) => reader.ReadInt16(),
                not null when propertyType == typeof(int) => reader.ReadInt32(),
                not null when propertyType == typeof(string) => reader.ReadAsciiString(header.NumToRead),
                not null when propertyType == typeof(float) => reader.ReadSingle(),
                not null when propertyType == typeof(double) => reader.ReadDouble(),
                _ => throw new ArgumentOutOfRangeException(nameof(propertyType), propertyType, "Unknown property type")
            };

            property.SetValue(this, valueToSet);
        }
    }
}