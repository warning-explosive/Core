namespace SpaceEngineers.Core.CompositionRoot.Roslyn.Test.Internals
{
    using Api;
    using Attributes;
    using Enumerations;
    using Xunit;

    /// <inheritdoc />
    [Lifestyle(EnLifestyle.Singleton)]
    internal class CodeFixVerifierImpl : ICodeFixVerifier
    {
        /// <inheritdoc />
        public void VerifyCodeFix(string expectedSource, string actualSource)
        {
            Assert.Equal(expectedSource, actualSource);
        }
    }
}