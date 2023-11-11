using System.Reflection;
using GdsSharp.Lib.Models.Parsing;

namespace GdsSharp.Lib;

public class GdsReader
{
    private readonly GdsBinaryReader _reader;
    private GdsHeader? _currentHeader;
    private List<IGdsRecord> _records = new();
    
    public GdsReader(Stream stream)
    {
        _reader = new GdsBinaryReader(stream);
    }

    public void Parse(CancellationToken token = default)
    {
        while (!token.IsCancellationRequested && _reader.BaseStream.Position < _reader.BaseStream.Length)
        {
            _currentHeader = ReadSimple<GdsHeader>();
            IGdsRecord record = _currentHeader.Code switch
            {
                0x0002 => ReadSimple<GdsRecordHeader>(),
                0x0102 => ReadSimple<GdsRecordBgnLib>(),
                0x0206 => ReadSimple<GdsRecordLibName>(),
                0x0305 => ReadSimple<GdsRecordUnits>(),
                0x1F06 => new GdsRecordRefLibs(_reader, _currentHeader),
                0x2006 => throw new NotImplementedException("FONTS"),
                0x2202 => throw new NotImplementedException("GENERATIONS"),
                0x2306 => throw new NotImplementedException("ATTRTABLE"),
                _ => throw new ArgumentOutOfRangeException(nameof(_currentHeader.Code), _currentHeader.Code, $"Unknown record code at position {_reader.BaseStream.Position:X}")
            };
            _records.Add(record);
        }
    }

    private T ReadSimple<T>() where T : IGdsSimpleRead, new()
    {
        if(_currentHeader is null && typeof(T) != typeof(GdsHeader))
            throw new InvalidOperationException("Cannot read a record without a header");
        
        var returnObject = new T();

        foreach (var property in typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.SetProperty))
        {
            if(property.GetSetMethod() == null) continue;
            
            var propertyType = property.PropertyType;

            object valueToSet = propertyType switch
            {
                not null when propertyType == typeof(ushort) => _reader.ReadUInt16(),
                not null when propertyType == typeof(uint) => _reader.ReadUInt32(),
                not null when propertyType == typeof(short) => _reader.ReadInt16(),
                not null when propertyType == typeof(int) => _reader.ReadInt32(),
                not null when propertyType == typeof(string) => _reader.ReadAsciiString(_currentHeader!.NumToRead),
                not null when propertyType == typeof(float) => _reader.ReadSingle(),
                not null when propertyType == typeof(double) => _reader.ReadDouble(),
                _ => throw new ArgumentOutOfRangeException(nameof(propertyType), propertyType, "Unknown property type")
            };
            
            property.SetValue(returnObject, valueToSet);
        }

        return returnObject;
    }
}