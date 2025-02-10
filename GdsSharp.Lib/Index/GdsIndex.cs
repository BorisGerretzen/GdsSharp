using GdsSharp.Lib.Lexing;
using GdsSharp.Lib.NonTerminals.Enum;
using GdsSharp.Lib.Terminals.Records;

namespace GdsSharp.Lib.Index;

public class GdsIndex
{
    public readonly IReadOnlyList<GdsIndexStructureEntry> Structures;
    
    private GdsIndex(List<GdsIndexStructureEntry> structures)
    {
        Structures = structures;
    }

    public static GdsIndex Create(GdsTokenStream stream)
    {
        var structures = new List<GdsIndexStructureEntry>();
        var index = new GdsIndex(structures);

        var structureStartOffset = 0L;
        long structureEndOffset;
        var structureName = string.Empty;
        var elements = new List<GdsIndexElementEntry>();

        var elementStartOffset = 0L;
        var elementType = GdsRecordNoDataType.Aref;

        while (stream.TryDequeue(out var token))
        {
            switch (token.Record)
            {
                case GdsRecordStrName nameRecord:
                    structureName = nameRecord.Value;
                    structureStartOffset = token.Offset + nameRecord.GetLength();
                    break;
                case GdsRecordNoData {Type: GdsRecordNoDataType.EndEl}:
                    elements.Add(new GdsIndexElementEntry(elementStartOffset, token.Offset, elementType));
                    break;
                case GdsRecordNoData { Type: GdsRecordNoDataType.EndStr }:
                    structureEndOffset = token.Offset;
                    structures.Add(new GdsIndexStructureEntry(structureName, structureStartOffset, structureEndOffset, elements));
                    break;
                case GdsRecordNoData { Type: var type } when IsElementType(type):
                    elementStartOffset = token.Offset;
                    elementType = type;
                    break;
            }
        }
        
        return index;
    }
    
    
    private static bool IsElementType(GdsRecordNoDataType type)
    {
        return type switch
        {
            GdsRecordNoDataType.Boundary => true,
            GdsRecordNoDataType.Path => true,
            GdsRecordNoDataType.Text => true,
            GdsRecordNoDataType.Node => true,
            GdsRecordNoDataType.Box => true,
            GdsRecordNoDataType.Sref => true,
            GdsRecordNoDataType.Aref => true,
            _ => false
        };
    }
}