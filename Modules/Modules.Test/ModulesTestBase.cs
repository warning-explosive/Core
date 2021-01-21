namespace SpaceEngineers.Core.Modules.Test
{
    using System;
    using System.Linq;
    using Abstractions;
    using AutoRegistration;
    using AutoRegistration.Abstractions;
    using Basics;
    using Basics.Test;
    using Core.SettingsManager.Extensions;
    using Xunit.Abstractions;

    /// <summary>
    /// Test base class
    /// </summary>
    public abstract class ModulesTestBase : BasicsTestBase
    {
        /// <summary> .ctor </summary>
        /// <param name="output">ITestOutputHelper</param>
        protected ModulesTestBase(ITestOutputHelper output)
            : base(output)
        {
            SolutionExtensions
                .ProjectFile()
                .Directory
                .EnsureNotNull($"Project directory {nameof(Modules)}.{nameof(Modules.Test)} not found")
                .StepInto("Settings")
                .SetupFileSystemSettingsDirectory();

            DependencyContainer = SetupDependencyContainer();
        }

        /// <summary>
        /// DependencyContainer
        /// </summary>
        protected IDependencyContainer DependencyContainer { get; }

        /// <summary>
        /// Setup DependencyContainer
        /// </summary>
        /// <param name="registration">Registration action</param>
        /// <returns>IDependencyContainer</returns>
        protected static IDependencyContainer SetupDependencyContainer(Action<IRegistrationContainer>? registration = null)
        {
            var options = new DependencyContainerOptions();
            options.OnRegistration += (s, e) =>
                                      {
                                          Registration(e.Registration);
                                          registration?.Invoke(e.Registration);
                                      };

            return AutoRegistration.DependencyContainer.Create(options);
        }

        private static void Registration(IRegistrationContainer container)
        {
            AssembliesExtensions
               .AllFromCurrentDomain()
               .Where(a => !a.IsDynamic)
               .SelectMany(a => a.GetTypes())
               .Where(type => type.IsClass
                           && (!type.IsGenericType || type.IsConstructedGenericType)
                           && typeof(ITestClassWithRegistration).IsAssignableFrom(type))
               .Select(type => Activator.CreateInstance(type).EnsureNotNull<ITestClassWithRegistration>("Test class hadn't instantiated. This class must have constructor without parameters."))
               .Each(registration => registration.Register(container));
        }
    }
}