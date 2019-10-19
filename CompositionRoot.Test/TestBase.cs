namespace SpaceEngineers.Core.CompositionRoot.Test
{
    using Xunit.Abstractions;

    public abstract class TestBase
    {
        protected TestBase(ITestOutputHelper output) { Output = output; }

        protected ITestOutputHelper Output { get; }
    }
}