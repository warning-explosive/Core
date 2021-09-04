namespace SpaceEngineers.Core.Basics.Test
{
    using Xunit.Abstractions;

    /// <summary>
    /// Unit test base class
    /// </summary>
    public abstract class BasicsTestBase
    {
        /// <summary> .ctor </summary>
        /// <param name="output">ITestOutputHelper</param>
        protected BasicsTestBase(ITestOutputHelper output)
        {
            Output = output;
        }

        /// <summary>
        /// ITestOutputHelper
        /// </summary>
        protected ITestOutputHelper Output { get; }
    }
}