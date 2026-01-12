namespace GdsSharp.Benchmarks.Obsolete.NonTerminals.Abstractions;

public interface IGdsLayeredElement : IGdsElement
{
    short Layer { get; set; }
}