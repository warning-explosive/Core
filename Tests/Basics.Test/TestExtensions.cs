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
            return Xunit.Sdk.FalseException.ForNonFalseValue(nameof(FalseException), null);
        }

        /// <summary> Create TrueException </summary>
        /// <returns>TrueException</returns>
        public static TrueException TrueException()
        {
            return Xunit.Sdk.TrueException.ForNonTrueValue(nameof(TrueException), null);
        }
    }
}