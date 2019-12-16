namespace SpaceEngineers.Core.CompositionRoot.RoslynAnalysis.Test.Internals
{
    using Api;
    using Attributes;
    using Enumerations;
    using Xunit;

    /// <inheritdoc />
    [Lifestyle(EnLifestyle.Singleton)]
    internal class CodeFixVerifyerImpl : ICodeFixVerifyer
    {
        /// <inheritdoc />
        public void VerifyCodeFix(string expectedSource, string actualSource)
        {
            Assert.Equal(expectedSource, actualSource);
        }
    }
}