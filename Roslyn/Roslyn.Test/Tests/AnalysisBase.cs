namespace SpaceEngineers.Core.Roslyn.Test.Tests
{
    using System;
    using System.Collections.Immutable;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using AutoRegistration;
    using AutoRegistration.Abstractions;
    using Microsoft.Build.Locator;
    using Xunit.Abstractions;

    /// <summary>
    /// AnalysisBase
    /// </summary>
    public abstract class AnalysisBase
    {
        private static Version[] _availableVersions;
        private static Version? _version;

        [SuppressMessage("Analysis", "CA1810", Justification = "Analysis test")]
        static AnalysisBase()
        {
            _availableVersions = MSBuildLocator
                .QueryVisualStudioInstances()
                .Select(it => it.Version)
                .OrderByDescending(it => it)
                .ToArray();

            var instance = MSBuildLocator
                .QueryVisualStudioInstances()
                .OrderByDescending(it => it.Version)
                .First();

            MSBuildLocator.RegisterInstance(instance);

            _version = instance.Version;
        }

        /// <summary> .cctor </summary>
        /// <param name="output">ITestOutputHelper</param>
        protected AnalysisBase(ITestOutputHelper output)
        {
            Output = output;

            Output.WriteLine($"Used framework version: {_version}");
            Output.WriteLine($"Available versions: {string.Join(", ", _availableVersions.Select(v => v.ToString()))}");

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