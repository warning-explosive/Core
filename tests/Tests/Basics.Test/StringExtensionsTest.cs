namespace SpaceEngineers.Core.Basics.Test;

using Xunit;
using Xunit.Abstractions;

public class StringExtensionsTest : BasicsTestBase
{
    public StringExtensionsTest(ITestOutputHelper output)
        : base(output) { }

    [Theory]
    [InlineData("qwerty", "Qwerty")]
    internal void StartFromCapitalLetterTest(string source, string expected)
    {
        Assert.Equal(expected, source.StartFromCapitalLetter());
    }
}