namespace SpaceEngineers.Core.AutoRegistration.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Basics;
    using Internals;
    using SimpleInjector;

    internal static class VerifiersExtensions
    {
        private const string SimpleInjectorAssemblyName = "SimpleInjector";
        private const string IsDecoratorMethodName = "IsDecorator";
        private static readonly string SimpleInjectorTypesTypeFullName = AssembliesExtensions.BuildName(SimpleInjectorAssemblyName, "Types");

        private static readonly MethodInfo IsDecoratorMethod = AssembliesExtensions
            .FindRequiredType(SimpleInjectorAssemblyName, SimpleInjectorTypesTypeFullName)
            .GetMethod(IsDecoratorMethodName, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
            .EnsureNotNull($"{IsDecoratorMethodName} should be found in {SimpleInjectorTypesTypeFullName} type");

        internal static bool IsDecorator(this ConstructorInfo constructorInfo, Type serviceType)
        {
            return (bool)IsDecoratorMethod.Invoke(null, new object[] { serviceType, constructorInfo });
        }

        internal static ConstructorInfo ResolutionConstructor(this Container container, Type component)
        {
            var constructor = container
                .Options
                .ConstructorResolutionBehavior
                .TryGetConstructor(component, out var errorMessage);

            return constructor != null
                ? constructor
                : throw new InvalidOperationException(errorMessage);
        }

        internal static IEnumerable<Type> RegisteredComponents(this Container container)
        {
            return container
                .GetCurrentRegistrations()
                .Select(DependencyInfo.RetrieveDependencyGraph)
                .SelectMany(info => info.ExtractFromGraph(dependency => dependency.ImplementationType))
                .Where(type => !type.IsAbstract)
                .Distinct();
        }
    }
}