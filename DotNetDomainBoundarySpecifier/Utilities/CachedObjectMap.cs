using System.Collections.Concurrent;

namespace DotNetDomainBoundarySpecifier.Utilities;

sealed class CachedObjectMap
{
    readonly ConcurrentDictionary<string, CacheItem> map = new();

    public TimeSpan Timeout { get; init; }

    public T AccessValue<T>(string key, Func<T> createFunc) where T : class
    {
        if (map.TryGetValue(key, out var record))
        {
            if (DateTime.Now - Timeout <= record.CreationTime)
            {
                return (T)record.Value;
            }
        }

        var value = createFunc();
        if (value is null)
        {
            return default;
        }

        map[key] = record = new() { CreationTime = DateTime.Now, Value = value };

        return (T)record.Value;
    }

    sealed class CacheItem
    {
        public DateTime CreationTime { get; init; }

        public object Value { get; init; }
    }
}