namespace SpaceEngineers.Core.Modules.Test
{
    using System;
    using AutoRegistration;
    using AutoRegistration.Abstractions;
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
            var options = new DependencyContainerOptions
                          {
                              AllowResolveInterception = true
                          };

            DependencyContainer = AutoRegistration.DependencyContainer.Create(options);
        }

        /// <summary>
        /// DependencyContainer
        /// </summary>
        protected IDependencyContainer DependencyContainer { get; }
    }
}