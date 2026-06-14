using KeyValueStore.Application.Abstractions;

namespace KeyValueStore.Application;

public sealed class SimpleStore : IKeyValueStore, IDisposable
{
    private readonly Dictionary<string, byte[]> _store = [];
    private readonly ReaderWriterLockSlim _lock = new();

    private long _setCount;
    private long _getCount;
    private long _deleteCount;

    public byte[]? Get(string key)
    {
        _lock.EnterReadLock();
        try
        {
            Interlocked.Increment(ref _getCount);
            return _store.TryGetValue(key, out var item) ? item : null;
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public void Set(string key, byte[] value)
    {
        _lock.EnterWriteLock();
        try
        {
            Interlocked.Increment(ref _setCount);
            _store[key] = value;
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    public void Delete(string key)
    {
        _lock.EnterWriteLock();
        try
        {
            Interlocked.Increment(ref _deleteCount);
            _store.Remove(key);
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    public (long setCount, long getCount, long deleteCount) GetStatistics()
    {
        return (_setCount, _getCount, _deleteCount);
    }

    public void Dispose()
    {
        _lock.Dispose();
    }
}
