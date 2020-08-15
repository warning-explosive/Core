namespace SpaceEngineers.Core.Modules.Test
{
    using System;
    using System.Linq;
    using System.Reflection;
    using Abstractions;
    using AutoRegistration;
    using AutoRegistration.Abstractions;
    using Basics;
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
                              RegistrationCallback = Registration
                          };

            DependencyContainer = AutoRegistration.DependencyContainer.Create(options);
        }

        /// <summary>
        /// DependencyContainer
        /// </summary>
        protected IDependencyContainer DependencyContainer { get; }

        private void Registration(IRegistrationContainer container)
        {
            GetType().Assembly
                     .GetTypes()
                     .Where(type => type.IsClass
                                 && (!type.IsGenericType || type.IsConstructedGenericType)
                                 && typeof(ITestClassWithRegistration).IsAssignableFrom(type))
                     .Select(type => Activator.CreateInstance(type).EnsureNotNull<ITestClassWithRegistration>("Test class hadn't instantiated. This class must have constructor without parameters."))
                     .Each(registration => registration.Register(container));
        }
    }
}