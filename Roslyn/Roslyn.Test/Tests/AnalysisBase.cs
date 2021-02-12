namespace SpaceEngineers.Core.Roslyn.Test.Tests
{
    using System.Collections.Immutable;
    using AutoRegistration;
    using AutoRegistration.Abstractions;
    using Microsoft.Build.Locator;
    using Xunit.Abstractions;

    /// <summary>
    /// AnalysisBase
    /// </summary>
    public abstract class AnalysisBase
    {
        static AnalysisBase()
        {
            MSBuildLocator.RegisterDefaults();
        }

        /// <summary> .cctor </summary>
        /// <param name="output">ITestOutputHelper</param>
        protected AnalysisBase(ITestOutputHelper output)
        {
            Output = output;

            DependencyContainer = AutoRegistration
                .DependencyContainer
                .Create(new DependencyContainerOptions
                {
                    ExcludedNamespaces = IgnoredNamespaces
                });
        }

        /// <summary>
        /// Excluded namespaces
        /// </summary>
        protected abstract ImmutableArray<string> IgnoredNamespaces { get; }

        /// <summary>
        /// ITestOutputHelper
        /// </summary>
        protected ITestOutputHelper Output { get; }

        /// <summary>
        /// IDependencyContainer
        /// </summary>
        protected IDependencyContainer DependencyContainer { get; }
    }
}