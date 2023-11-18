namespace GdsSharp.Lib.Abstractions;

public interface IGdsElement
{
    bool ExternalData { get; set; }
    bool TemplateData { get; set; }
    int PlexNumber { get; set; }
}