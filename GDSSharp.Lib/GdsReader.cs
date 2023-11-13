using System.Reflection;
using GdsSharp.Lib.Models.Parsing;
using GdsSharp.Lib.Parsing.Models;

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
                0x4000 => new GdsRecordNoData{Type = GdsRecordNoDataType.EndLib},
                0x0502 => ReadSimple<GdsRecordBgnStr>(),
                0x0606 => ReadSimple<GdsRecordStrName>(),
                0x0700 => new GdsRecordNoData{Type = GdsRecordNoDataType.EndStr},
                0x0800 => new GdsRecordNoData{Type = GdsRecordNoDataType.Boundary},
                0x0900 => new GdsRecordNoData{Type = GdsRecordNoDataType.Path},
                0x0A00 => new GdsRecordNoData{Type = GdsRecordNoDataType.Sref},
                0x0B00 => new GdsRecordNoData{Type = GdsRecordNoDataType.Aref},
                0x0C00 => new GdsRecordNoData{Type = GdsRecordNoDataType.Text},
                0x0D02 => ReadSimple<GdsRecordLayer>(),
                0x0E02 => ReadSimple<GdsRecordDataType>(),
                0x0F03 => ReadSimple<GdsRecordWidth>(),
                0x1003 => new GdsRecordXy(_reader, _currentHeader),
                0x1100 => new GdsRecordNoData{Type = GdsRecordNoDataType.EndEl},
                0x1206 => ReadSimple<GdsRecordSName>(),
                0x1302 => ReadSimple<GdsRecordColRow>(),
                0x1500 => new GdsRecordNoData{Type = GdsRecordNoDataType.Node},
                0x1602 => ReadSimple<GdsRecordTextType>(),
                0x1701 => new GdsRecordPresentation(_reader, _currentHeader),
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