namespace SpaceEngineers.Core.Basics.Test;

using Xunit;
using Xunit.Abstractions;

public class StringExtensionsTest(ITestOutputHelper output) : BasicsTestBase(output)
{
    [Theory]
    [InlineData("qwerty", "Qwerty")]
    internal void StartFromCapitalLetterTest(string source, string expected)
    {
        Assert.Equal(expected, source.StartFromCapitalLetter());
    }
}