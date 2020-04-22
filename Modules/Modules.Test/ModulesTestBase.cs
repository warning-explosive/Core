namespace SpaceEngineers.Core.Modules.Test
{
    using System;
    using AutoRegistration;
    using Basics.Test;
    using Xunit.Abstractions;

    /// <summary>
    /// Test base class
    /// </summary>
    public abstract class ModulesTestBase : BasicsTestBase
    {
        /// <summary> .ctor </summary>
        /// <param name="output">ITestOutputHelper</param>
        /// <exception cref="InvalidOperationException">AppDomain.CurrentDomain == null</exception>
        protected ModulesTestBase(ITestOutputHelper output)
            : base(output)
        {
            DependencyContainer = DependencyContainer.Default(typeof(ModulesTestBase).Assembly);
        }

        /// <summary>
        /// DependencyContainer
        /// </summary>
        protected DependencyContainer DependencyContainer { get; }
    }
}