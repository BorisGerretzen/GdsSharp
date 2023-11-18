namespace GdsSharp.Lib.Abstractions;

public interface IGdsLayeredElement : IGdsElement
{
    short Layer { get; set; }
}