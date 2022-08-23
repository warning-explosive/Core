namespace SpaceEngineers.Core.Roslyn.Test.Tests
{
    using System;
    using System.Collections.Immutable;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using CompositionRoot;
    using CompositionRoot.Api.Abstractions;
    using Core.Test.Api;
    using Core.Test.Api.ClassFixtures;
    using Microsoft.Build.Locator;
    using Registrations;
    using Xunit.Abstractions;

    /// <summary>
    /// AnalysisBase
    /// </summary>
    public abstract class AnalysisBase : TestBase
    {
        private static readonly Version[] AvailableVersions;
        private static readonly Version? Version;

        [SuppressMessage("Analysis", "CA1810", Justification = "Analysis test")]
        static AnalysisBase()
        {
            AvailableVersions = MSBuildLocator
                .QueryVisualStudioInstances()
                .Select(it => it.Version)
                .OrderByDescending(it => it)
                .ToArray();

            var instance = MSBuildLocator
                .QueryVisualStudioInstances()
                .OrderByDescending(it => it.Version)
                .First();

            MSBuildLocator.RegisterInstance(instance);

            Version = instance.Version;
        }

        /// <summary> .cctor </summary>
        /// <param name="output">ITestOutputHelper</param>
        /// <param name="fixture">TestFixture</param>
        protected AnalysisBase(ITestOutputHelper output, TestFixture fixture)
            : base(output, fixture)
        {
            output.WriteLine($"Used framework version: {Version}");
            output.WriteLine($"Available versions: {string.Join(", ", AvailableVersions.Select(v => v.ToString()))}");

            var options = new DependencyContainerOptions()
                .WithManualRegistrations(new AnalyzersManualRegistration())
                .WithExcludedNamespaces(IgnoredNamespaces.ToArray());

            DependencyContainer = fixture.Container(output, options);
        }

        /// <summary>
        /// Excluded namespaces
        /// </summary>
        protected abstract ImmutableArray<string> IgnoredNamespaces { get; }

        /// <summary>
        /// IDependencyContainer
        /// </summary>
        protected IDependencyContainer DependencyContainer { get; }
    }
}