namespace SpaceEngineers.Core.CompositionRoot.Test
{
    using System;
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
            if (AppDomain.CurrentDomain == null)
            {
                throw new InvalidOperationException("CurrentDomain is null");
            }

            DependencyContainer = new DependencyContainer(AppDomain.CurrentDomain.GetAssemblies());
        }

        /// <summary>
        /// DependencyContainer
        /// </summary>
        protected DependencyContainer DependencyContainer { get; }
    }
}