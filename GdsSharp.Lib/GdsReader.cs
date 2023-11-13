using GdsSharp.Lib.Parsing;
using GdsSharp.Lib.Parsing.Tokens;

namespace GdsSharp.Lib;

public class GdsReader
{
    private readonly GdsBinaryReader _reader;
    private GdsHeader? _currentHeader;

    public GdsReader(Stream stream)
    {
        _reader = new GdsBinaryReader(stream);
    }

    public IEnumerable<IGdsRecord> Tokenize(CancellationToken token = default)
    {
        while (!token.IsCancellationRequested && _reader.BaseStream.Position < _reader.BaseStream.Length)
        {
            _currentHeader = new GdsHeader();
            ((IGdsSimpleRead)_currentHeader).Read(_reader, _currentHeader);

            // Stop when padding is reached
            if (_currentHeader.Code == 0 && _currentHeader.Length == 0) yield break;

            IGdsRecord record = _currentHeader.Code switch
            {
                0x0002 => new GdsRecordHeader(),
                0x0102 => new GdsRecordBgnLib(),
                0x0206 => new GdsRecordLibName(),
                0x0305 => new GdsRecordUnits(),
                0x0400 => new GdsRecordNoData { Type = GdsRecordNoDataType.EndLib },
                0x0502 => new GdsRecordBgnStr(),
                0x0606 => new GdsRecordStrName(),
                0x0700 => new GdsRecordNoData { Type = GdsRecordNoDataType.EndStr },
                0x0800 => new GdsRecordNoData { Type = GdsRecordNoDataType.Boundary },
                0x0900 => new GdsRecordNoData { Type = GdsRecordNoDataType.Path },
                0x0A00 => new GdsRecordNoData { Type = GdsRecordNoDataType.Sref },
                0x0B00 => new GdsRecordNoData { Type = GdsRecordNoDataType.Aref },
                0x0C00 => new GdsRecordNoData { Type = GdsRecordNoDataType.Text },
                0x0D02 => new GdsRecordLayer(),
                0x0E02 => new GdsRecordDataType(),
                0x0F03 => new GdsRecordWidth(),
                0x1003 => new GdsRecordXy(),
                0x1100 => new GdsRecordNoData { Type = GdsRecordNoDataType.EndEl },
                0x1206 => new GdsRecordSName(),
                0x1302 => new GdsRecordColRow(),
                0x1500 => new GdsRecordNoData { Type = GdsRecordNoDataType.Node },
                0x1602 => new GdsRecordTextType(),
                0x1701 => new GdsRecordPresentation(),
                0x1906 => new GdsRecordString(),
                0x1A01 => new GdsRecordSTrans(),
                0x1B05 => new GdsRecordMag(),
                0x1C05 => new GdsRecordAngle(),
                0x1F06 => new GdsRecordRefLibs(),
                0x2006 => new GdsRecordFonts(),
                0x2102 => new GdsRecordPathType(),
                0x2202 => new GdsRecordGenerations(),
                0x2306 => throw new NotImplementedException("ATTRTABLE"),
                0x2601 => new GdsRecordElFlags(),
                0x2A02 => new GdsRecordNodeType(),
                0x2B02 => new GdsRecordPropAttr(),
                0x2C06 => new GdsRecordPropValue(),
                0x2D00 => new GdsRecordNoData { Type = GdsRecordNoDataType.Box },
                0x2E02 => new GdsRecordBoxType(),
                0x2F03 => new GdsRecordPlex(),
                0x3202 => new GdsRecordTapeNum(),
                0x3302 => new GdsRecordTapeCode(),
                0x3602 => new GdsRecordFormat(),
                0x3706 => new GdsRecordMask(),
                0x3800 => new GdsRecordNoData { Type = GdsRecordNoDataType.EndMasks },
                _ => throw new ArgumentOutOfRangeException(nameof(_currentHeader.Code), $"0x{_currentHeader.Code:X}", $"Unknown record code at position 0x{_reader.BaseStream.Position:X} ({_reader.BaseStream.Position})")
            };

            if (record is IGdsReadableRecord readableRecord) readableRecord.Read(_reader, _currentHeader);

            yield return record;
        }
    }
}