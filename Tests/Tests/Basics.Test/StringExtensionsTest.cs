namespace SpaceEngineers.Core.Basics.Test
{
    using Xunit;
    using Xunit.Abstractions;

    /// <summary>
    /// StringExtensions class test
    /// </summary>
    public class StringExtensionsTest : BasicsTestBase
    {
        /// <summary> .ctor </summary>
        /// <param name="output">ITestOutputHelper</param>
        public StringExtensionsTest(ITestOutputHelper output)
            : base(output) { }

        [Theory]
        [InlineData("qwerty", "Qwerty")]
        internal void StartFromCapitalLetterTest(string source, string expected)
        {
            Assert.Equal(expected, source.StartFromCapitalLetter());
        }
    }
}