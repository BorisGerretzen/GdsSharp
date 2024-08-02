using System.Linq.Expressions;
using System.Reflection;
using GdsSharp.Lib.Terminals.Abstractions;
using GdsSharp.Lib.Terminals.Records;

namespace GdsSharp.Lib.Abstractions;

public class GdsStreamOperator
{
    protected static readonly Dictionary<ushort, Func<IGdsRecord>> Activators = new();

    /// <summary>
    ///     Initializes activators for all records.
    /// </summary>
    static GdsStreamOperator()
    {
        var assembly = Assembly.GetAssembly(typeof(GdsTokenizer));
        if (assembly is null) throw new InvalidOperationException("Could not get assembly");

        // Get compiled activator for all records
        var recordTypes = assembly.GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false } && typeof(IGdsRecord).IsAssignableFrom(t));
        foreach (var recordType in recordTypes)
        {
            var activator = Expression.Lambda<Func<IGdsRecord>>(Expression.New(recordType)).Compile();
            var record = activator.Invoke();
            if (record is null) throw new InvalidOperationException($"Could not get activator for {recordType.Name}");
            Activators.Add(record.Code, activator);
        }

        // Add activator for no data records
        foreach (var value in Enum.GetValues<GdsRecordNoDataType>())
            Activators.Add((ushort)value, () => new GdsRecordNoData { Type = value });
    }
}