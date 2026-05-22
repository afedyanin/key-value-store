namespace KeyValueStore.Application.Tests;

public class CommandParserTests
{
    [Fact]
    public void CanParseCommandString()
    {
        var cmd = CommandParser.Parse("SET User Data");

        Assert.Equal("SET", cmd.Command);
        Assert.Equal("User", cmd.Key);
        Assert.Equal("Data", cmd.Value);
    }
}
