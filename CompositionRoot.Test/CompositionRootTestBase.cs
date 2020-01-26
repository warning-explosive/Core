namespace SpaceEngineers.Core.CompositionRoot.Test
{
    using System;
    using Basics;
    using Basics.Test;
    using Xunit.Abstractions;

    /// <summary>
    /// CompositionRoot base test class
    /// </summary>
    public abstract class CompositionRootTestBase : BasicsTestBase
    {
        /// <summary> .ctor </summary>
        /// <param name="output">ITestOutputHelper</param>
        /// <exception cref="InvalidOperationException">AppDomain.CurrentDomain == null</exception>
        protected CompositionRootTestBase(ITestOutputHelper output)
            : base(output)
        {
            var assemblies = AppDomain.CurrentDomain
                                      .TryExtractNotNullable(() => new InvalidOperationException("CurrentDomain is null"))
                                      .GetAssemblies();

            DependencyContainer = new DependencyContainer(assemblies);
        }

        /// <summary>
        /// DependencyContainer
        /// </summary>
        protected DependencyContainer DependencyContainer { get; }
    }
}