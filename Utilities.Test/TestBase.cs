namespace SpaceEngineers.Core.Utilities.Test
{
    using Xunit.Abstractions;

    public abstract class TestBase
    {
        protected ITestOutputHelper Output { get; }

        protected TestBase(ITestOutputHelper output)
        {
            Output = output;
        }
    }
}