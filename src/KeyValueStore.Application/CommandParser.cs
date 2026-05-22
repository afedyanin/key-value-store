namespace KeyValueStore.Application;

public static class CommandParser
{
    private const char ArgDelimiter = ' ';

    public static ParsedCommand Parse(ReadOnlySpan<char> input)
    {
        input = input.Trim();

        if (input.IsEmpty)
        {
            return default;
        }

        var firstSpace = input.IndexOf(ArgDelimiter);

        if (firstSpace == -1)
        {
            return default;
        }

        var command = input[..firstSpace];
        var remaining = input[(firstSpace + 1)..].TrimStart();

        if (remaining.IsEmpty)
        {
            return default;
        }

        var secondSpace = remaining.IndexOf(ArgDelimiter);

        if (secondSpace == -1)
        {
            return new ParsedCommand(command, remaining, []);
        }

        var key = remaining[..secondSpace];
        var value = remaining[(secondSpace + 1)..].TrimStart();

        return new ParsedCommand(command, key, value);
    }
}
