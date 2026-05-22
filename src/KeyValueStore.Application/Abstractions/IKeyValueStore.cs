namespace KeyValueStore.Application.Abstractions;

public interface IKeyValueStore
{
    public void Set(string key, byte[] value);

    public byte[]? Get(string key);

    public void Delete(string key);
}
