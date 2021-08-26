namespace SpaceEngineers.Core.Test.Api
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using Basics;
    using Basics.Exceptions;
    using ClassFixtures;
    using CompositionRoot;
    using CompositionRoot.Api.Abstractions;
    using Settings;

    /// <summary>
    /// CompositionRoot extensions
    /// </summary>
    public static class CompositionRootExtensions
    {
        private static readonly Encoding Encoding = new UTF8Encoding(true);
        private static Type? _type;

        /// <summary>
        /// Use DI container from settings
        /// </summary>
        /// <param name="options">DependencyContainerOptions</param>
        /// <returns>Dependency container implementation producer</returns>
        public static Func<IDependencyContainerImplementation> UseGenericContainer(this DependencyContainerOptions options)
        {
            _type ??= FindRequiredType();

            return () => (IDependencyContainerImplementation)Activator.CreateInstance(_type);
        }

        private static Type FindRequiredType()
        {
            var settingsDirectory = new ModulesTestFixture().SettingsDirectory;

            var settingsPath = Path.ChangeExtension(Path.Combine(settingsDirectory.FullName, nameof(GenericContainerSettings)), "yaml");

            var serialized = File.ReadAllLines(settingsPath, Encoding);

            var implementationFullName = serialized
                .SingleOrDefault(it => it.Contains(nameof(GenericContainerSettings.DependencyContainerImplementationFullName), StringComparison.OrdinalIgnoreCase))
                .EnsureNotNull($"{nameof(GenericContainerSettings)} should have {nameof(GenericContainerSettings.DependencyContainerImplementationFullName)} property definition")
                .Split(':', StringSplitOptions.RemoveEmptyEntries)
                .Skip(1)
                .SingleOrDefault()
                .EnsureNotNull($"{nameof(GenericContainerSettings)} should have {nameof(GenericContainerSettings.DependencyContainerImplementationFullName)} property value definition")
                .Trim();

            var assemblyNameParts = implementationFullName
                .Split(".", StringSplitOptions.RemoveEmptyEntries)
                .SkipLast(1)
                .TakeWhile(part => !part.Contains("+", StringComparison.OrdinalIgnoreCase))
                .ToList();

            return FindRequiredType(assemblyNameParts, implementationFullName);
        }

        private static Type FindRequiredType(IReadOnlyCollection<string> assemblyNameParts, string implementationFullName)
        {
            var possibleAssemblyNames = assemblyNameParts
                .Select((_, index) => assemblyNameParts.Take(index + 1).ToString("."))
                .Reverse();

            foreach (var assemblyName in possibleAssemblyNames)
            {
                var type = AssembliesExtensions.FindType(assemblyName, implementationFullName);

                if (type != null)
                {
                    return type;
                }
            }

            throw new NotFoundException($"Type {implementationFullName} should be found in current {nameof(AppDomain)}");
        }
    }
}