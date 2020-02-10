namespace SpaceEngineers.Core.Roslyn.Test.Internals
{
    using Api;
    using AutoWiringApi.Attributes;
    using AutoWiringApi.Enumerations;
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