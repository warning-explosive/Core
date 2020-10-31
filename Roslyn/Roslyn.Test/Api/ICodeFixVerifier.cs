namespace SpaceEngineers.Core.Roslyn.Test.Api
{
    using AutoWiringApi.Abstractions;

    /// <summary>
    /// Verifies code fix results
    /// </summary>
    public interface ICodeFixVerifier : IResolvable
    {
        /// <summary>
        /// Verify code fix results
        /// </summary>
        /// <param name="expectedSource">Expected result after code fix execution</param>
        /// <param name="actualSource">Actual result after code fix execution</param>
        void VerifyCodeFix(string expectedSource, string actualSource);
    }
}