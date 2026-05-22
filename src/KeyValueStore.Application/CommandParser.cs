namespace KeyValueStore.Application;

public static class CommandParser
{
    public static ActionCommand Parse(ReadOnlySpan<char> commandString)
    {
        return new ActionCommand();
    }
}
