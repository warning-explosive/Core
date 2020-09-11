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

        private static void SetupFileSystemSettingsDirectory()
        {
            var fileSystemSettingsDirectory = Path.Combine(SolutionExtensions.ProjectDirectory(), "Settings");
            Environment.SetEnvironmentVariable(Constants.FileSystemSettingsDirectory,
                                               fileSystemSettingsDirectory,
                                               EnvironmentVariableTarget.Process);
        }

        private IDependencyContainer SetupDependencyContainer()
        {
            var options = new DependencyContainerOptions
                          {
                              RegistrationCallback = Registration
                          };

            return AutoRegistration.DependencyContainer.Create(options);
        }

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