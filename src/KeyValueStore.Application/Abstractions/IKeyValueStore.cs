namespace KeyValueStore.Application.Abstractions;

public interface IKeyValueStore
{
    public void Set(string key, UserProfile profile);

    public UserProfile? Get(string key);

    public void Delete(string key);
}
