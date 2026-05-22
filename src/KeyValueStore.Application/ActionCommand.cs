namespace KeyValueStore.Application;

public readonly ref struct ActionCommand
{
    public ReadOnlySpan<char> Command { get; init; }
    public ReadOnlySpan<char> Key { get; init; }
    public ReadOnlySpan<char> Value { get; init; }
}
