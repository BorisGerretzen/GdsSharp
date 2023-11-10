using System.Reflection;
using GdsSharp.Lib.Models.Parsing;

namespace GdsSharp.Lib;

public class GdsReader
{
    private readonly BigEndianBinaryReader _reader;

    public GdsReader(Stream stream)
    {
        _reader = new BigEndianBinaryReader(stream);
    }

    public void Parse(CancellationToken token = default)
    {
        while (!token.IsCancellationRequested && _reader.BaseStream.Position < _reader.BaseStream.Length)
        {
            var header = ReadSimple<GdsHeader>();
            IGdsRecord record = header.Code switch
            {
                0x0002 => ReadSimple<GdsRecordHeader>(),
                0x0102 => ReadSimple<GdsRecordBgnLib>(),
                _ => throw new ArgumentOutOfRangeException(nameof(header.Code), header.Code, "Unknown record code")
            };
        }
    }

    private T ReadSimple<T>() where T : new()
    {
        var value = new T();

        foreach (var property in typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.SetProperty))
        {
            if(property.GetSetMethod() == null) continue;
            
            var propertyType = property.PropertyType;
            
            switch (propertyType)
            {
                case not null when propertyType == typeof(ushort):
                {
                    var propValue = _reader.ReadUInt16();
                    property.SetValue(value, propValue);
                    break;
                }
                case not null when propertyType == typeof(uint):
                {
                    var propValue = _reader.ReadUInt32();
                    property.SetValue(value, propValue);
                    break;
                }
                case not null when propertyType == typeof(short):
                {
                    var propValue = _reader.ReadInt16();
                    property.SetValue(value, propValue);
                    break;
                }
                case not null when propertyType == typeof(int):
                {
                    var propValue = _reader.ReadInt32();
                    property.SetValue(value, propValue);
                    break;
                }
                default:
                {
                    throw new Exception($"Type {propertyType} is not supported");
                }
            }
        }

        return value;
    }
}