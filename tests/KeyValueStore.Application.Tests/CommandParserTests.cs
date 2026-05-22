namespace KeyValueStore.Application.Tests;

public class CommandParserTests
{
    [Fact]
    public void Parse_ThreeArguments_ReturnsAllComponents()
    {
        var input = "SET user:1 data";

        var result = CommandParser.Parse(input.AsSpan());

        Assert.Equal("SET", result.Command.ToString());
        Assert.Equal("user:1", result.Key.ToString());
        Assert.Equal("data", result.Value.ToString());
    }

    [Fact]
    public void Parse_TwoArguments_ReturnsEmptyValue()
    {
        var input = "GET user:1";

        var result = CommandParser.Parse(input.AsSpan());

        Assert.Equal("GET", result.Command.ToString());
        Assert.Equal("user:1", result.Key.ToString());
        Assert.True(result.Value.IsEmpty);
    }

    [Fact]
    public void Parse_MultipleSpaces_TrimsCorrectly()
    {
        var input = "  SET    user:2      my_value  ";

        var result = CommandParser.Parse(input.AsSpan());

        Assert.Equal("SET", result.Command.ToString());
        Assert.Equal("user:2", result.Key.ToString());
        Assert.Equal("my_value", result.Value.ToString());
    }

    [Fact]
    public void Parse_OnlyCommand_ReturnsDefault()
    {
        var input = "PING";

        var result = CommandParser.Parse(input.AsSpan());

        Assert.True(result.Command.IsEmpty);
        Assert.True(result.Key.IsEmpty);
        Assert.True(result.Value.IsEmpty);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Parse_EmptyOrWhitespaceInput_ReturnsDefault(string input)
    {
        var result = CommandParser.Parse(input.AsSpan());

        Assert.True(result.Command.IsEmpty);
        Assert.True(result.Key.IsEmpty);
        Assert.True(result.Value.IsEmpty);
    }

    [Fact]
    public void Parse_ValueContainsSpaces_PreservesValueSpaces()
    {
        var input = "SET message Hello World From C#";

        var result = CommandParser.Parse(input.AsSpan());

        Assert.Equal("SET", result.Command.ToString());
        Assert.Equal("message", result.Key.ToString());
        Assert.Equal("Hello World From C#", result.Value.ToString());
    }
}
