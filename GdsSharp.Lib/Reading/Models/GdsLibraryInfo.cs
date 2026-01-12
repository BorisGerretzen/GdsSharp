using GdsSharp.Lib.Reading.Enum;

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
    GdsFormatType? FormatType)
{
    public static readonly GdsLibraryInfo Default = new(
        Version: 700,
        Name: "DefaultLibrary",
        ModificationTime: DateTime.Now,
        AccessTime: DateTime.Now,
        ReferencedLibraries: [],
        Fonts: [],
        AttributeDefinitionFile: null,
        Generations: null,
        UserUnits: 1,
        PhysicalUnits: 1e-8,
        FormatType: GdsFormatType.GdsArchive);
}