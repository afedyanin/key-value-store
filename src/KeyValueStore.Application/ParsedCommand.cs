namespace KeyValueStore.Application;

public readonly ref struct ParsedCommand
{
    public readonly ReadOnlySpan<char> Command { get; init; }
    public readonly ReadOnlySpan<char> Key { get; init; }
    public readonly ReadOnlySpan<char> Value { get; init; }

    public ParsedCommand(ReadOnlySpan<char> command, ReadOnlySpan<char> key, ReadOnlySpan<char> value)
    {
        Command = command;
        Key = key;
        Value = value;
    }
}