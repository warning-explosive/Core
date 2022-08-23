namespace SpaceEngineers.Core.Test.Api
{
    using ClassFixtures;
    using Xunit;
    using Xunit.Abstractions;

    /// <summary>
    /// TestBase
    /// </summary>
    public abstract class TestBase : IClassFixture<TestFixture>
    {
        /// <summary> .cctor </summary>
        /// <param name="output">ITestOutputHelper</param>
        /// <param name="fixture">TestFixture</param>
        protected TestBase(ITestOutputHelper output, TestFixture fixture)
        {
            Output = output;
            Fixture = fixture;
        }

        /// <summary>
        /// ITestOutputHelper
        /// </summary>
        protected ITestOutputHelper Output { get; }

        /// <summary>
        /// TestFixture
        /// </summary>
        protected TestFixture Fixture { get; }
    }
}