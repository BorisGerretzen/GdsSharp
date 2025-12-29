namespace GdsSharp.Lib.Reading.TokenStream;

public class GdsRecordTypes
{
    public const ushort Header = 0x0002;
    public const ushort BeginLibrary = 0x0102;
    public const ushort LibraryName = 0x0206;
    public const ushort Units = 0x0305;
    public const ushort EndLibrary = 0x0400;
    public const ushort BeginStruct = 0x0502;
    public const ushort StructName = 0x0606;
    public const ushort EndStruct = 0x0700;
    public const ushort Boundary = 0x0800;
    public const ushort Path = 0x0900;
    public const ushort StructureReference = 0x0A00;
    public const ushort ArrayReference = 0x0B00;
    public const ushort Text = 0x0C00;
    public const ushort Layer = 0x0D02;
    public const ushort DataType = 0x0E02;
    public const ushort Width = 0x0F03;
    public const ushort Xy = 0x1003;
    public const ushort EndElement = 0x1100;
    public const ushort StructureName = 0x1206;
    public const ushort ColumnRow = 0x1302;
    public const ushort Node = 0x1500;
    public const ushort TextType = 0x1602;
    public const ushort Presentation = 0x1701;
    public const ushort String = 0x1906;
    public const ushort Strans = 0x1A01;
    public const ushort Magnification = 0x1B05;
    public const ushort Angle = 0x1C05;
    public const ushort ReferencedLibraries = 0x1F06;
    public const ushort Fonts = 0x2006;
    public const ushort PathType = 0x2102;
    public const ushort Generations = 0x2202;
    public const ushort AttributeTable = 0x2306;
    public const ushort ElementFlags = 0x2601;
    public const ushort NodeType = 0x2A02;
    public const ushort PropertyAttribute = 0x2B02;
    public const ushort PropertyValue = 0x2C06;
    public const ushort Box = 0x2D00;
    public const ushort BoxType = 0x2E02;
    public const ushort Plex = 0x2F03;
    public const ushort TapeNumber = 0x3202;
    public const ushort TapeCode = 0x3302;
    public const ushort Format = 0x3602;
    public const ushort Mask = 0x3706;
    public const ushort EndMasks = 0x3800;
}