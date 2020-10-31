namespace SpaceEngineers.Core.Roslyn.Test.Tests
{
    using System;
    using System.Collections.Immutable;
    using System.Threading;
    using AutoRegistration;
    using AutoRegistration.Abstractions;
    using Microsoft.Build.Locator;
    using Xunit;
    using Xunit.Abstractions;

    /// <summary>
    /// AnalysisBase
    /// </summary>
    public abstract class AnalysisBase : IDisposable
    {
        private static readonly ManualResetEventSlim Event = new ManualResetEventSlim(true);

        private static readonly Lazy<VisualStudioInstance> VisualStudioInstance
            = new Lazy<VisualStudioInstance>(MSBuildLocator.RegisterDefaults, LazyThreadSafetyMode.ExecutionAndPublication);

        /// <summary> .cctor </summary>
        /// <param name="output">ITestOutputHelper</param>
        protected AnalysisBase(ITestOutputHelper output)
        {
            Event.Wait();
            Event.Reset();

            Assert.NotNull(VisualStudioInstance.Value);

            Output = output;

            DependencyContainer = AutoRegistration.DependencyContainer
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

        /// <inheritdoc />
        public void Dispose()
        {
            try
            {
                MSBuildLocator.Unregister();
            }
            finally
            {
                Event.Set();
            }
        }
    }
}