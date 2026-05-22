using KeyValueStore.Application.Abstractions;

namespace KeyValueStore.Application;

public class SimpleStore : IKeyValueStore
{
    private readonly Dictionary<string, byte[]> _store = [];

    public byte[]? Get(string key)
    {
        _store.TryGetValue(key, out var item);
        return item;
    }

    public void Set(string key, byte[] value)
    {
        _store[key] = value;
    }

    public void Delete(string key)
    {
        _store.Remove(key);
    }
}
