namespace GdsSharp.Lib.Abstractions;

public interface IGdsElement
{
    bool ExternalData { get; set; }
    bool TemplateData { get; set; }
    short PlexNumber { get; set; }
}