namespace SpaceEngineers.Core.Basics.Test
{
    using Xunit.Sdk;

    /// <summary>
    /// extensions for tests
    /// </summary>
    public static class TestExtensions
    {
        /// <summary> Create FalseException </summary>
        /// <returns>FalseException</returns>
        public static FalseException FalseException()
        {
            return new FalseException(nameof(FalseException), null);
        }

        /// <summary> Create TrueException </summary>
        /// <returns>TrueException</returns>
        public static TrueException TrueException()
        {
            return new TrueException(nameof(TrueException), null);
        }
    }
}