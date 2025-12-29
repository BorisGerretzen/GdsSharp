using GdsSharp.Lib.Obsolete.NonTerminals.Enum;

namespace GdsSharp.Lib.Reading.Models;

public readonly record struct GdsLibraryInfo(
    short Version,
    string Name,
    DateTime ModificationTime,
    DateTime AccessTime,
    IReadOnlyCollection<string> ReferencedLibraries,
    IReadOnlyCollection<string> Fonts,
    string? AttributeDefinitionFile,
    short? Generations,
    double UserUnits,
    double PhysicalUnits,
    GdsFormatType FormatType);