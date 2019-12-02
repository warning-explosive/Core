namespace SpaceEngineers.Core.Basics.Test
{
    using Xunit.Abstractions;

    public abstract class BasicsTestBase
    {
        protected BasicsTestBase(ITestOutputHelper output)
        {
            Output = output;
        }

        protected ITestOutputHelper Output { get; }
    }
}