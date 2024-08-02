namespace GdsSharp.Lib.NonTerminals.Abstractions;

public interface IGdsLayeredElement : IGdsElement
{
    short Layer { get; set; }
}