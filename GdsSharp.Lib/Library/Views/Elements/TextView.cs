using GdsSharp.Lib.Reading.Enum;
using GdsSharp.Lib.Reading.Models;

namespace GdsSharp.Lib.Library.Views.Elements;

public readonly struct TextView
{
    private readonly GdsLibrary _library;
    private readonly int _textIndex;

    public readonly int ElementIndex;
    public short Layer => _library.Texts[_textIndex].Layer;
    public short TextType => _library.Texts[_textIndex].TextType;
    public PresentationInfo? Presentation => _library.Texts[_textIndex].Presentation;
    public GdsPathType? PathType => _library.Texts[_textIndex].PathType;
    public int? Width => _library.Texts[_textIndex].Width;
    public GdsStransInfo? Strans => _library.Texts[_textIndex].Strans;
    public GdsPoint Origin => _library.Texts[_textIndex].Origin;
    public string Text => _library.Texts[_textIndex].Text;

    internal TextView(GdsLibrary library, int elementIndex)
    {
        _library = library;
        ElementIndex = elementIndex;
        _textIndex = library.Elements[elementIndex].Index;
    }
}