namespace SpaceEngineers.Core.Modules.Test
{
    using System;
    using System.IO;
    using System.Linq;
    using Abstractions;
    using AutoRegistration;
    using AutoRegistration.Abstractions;
    using Basics;
    using Basics.Test;
    using Core.SettingsManager;
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
            SetupFileSystemSettingsDirectory();
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
            var options = new DependencyContainerOptions
                          {
                              RegistrationCallback = container =>
                                                     {
                                                         Registration(container);
                                                         registration?.Invoke(container);
                                                     }
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

        private static void SetupFileSystemSettingsDirectory()
        {
            var fileSystemSettingsDirectory = Path.Combine(SolutionExtensions.ProjectDirectory(), "Settings");
            Environment.SetEnvironmentVariable(Constants.FileSystemSettingsDirectory,
                                               fileSystemSettingsDirectory,
                                               EnvironmentVariableTarget.Process);
        }
    }
}