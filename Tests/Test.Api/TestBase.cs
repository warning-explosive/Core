namespace SpaceEngineers.Core.Test.Api
{
    using ClassFixtures;
    using Xunit;
    using Xunit.Abstractions;

    /// <summary>
    /// TestBase
    /// </summary>
    public abstract class TestBase : IClassFixture<ModulesTestFixture>
    {
        /// <summary> .cctor </summary>
        /// <param name="output">ITestOutputHelper</param>
        /// <param name="fixture">ModulesTestFixture</param>
        protected TestBase(ITestOutputHelper output, ModulesTestFixture fixture)
        {
            Output = output;
            Fixture = fixture;
        }

        /// <summary>
        /// ITestOutputHelper
        /// </summary>
        protected ITestOutputHelper Output { get; }

        /// <summary>
        /// ModulesTestFixture
        /// </summary>
        protected ModulesTestFixture Fixture { get; }
    }
}