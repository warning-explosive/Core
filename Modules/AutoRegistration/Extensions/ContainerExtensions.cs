namespace SpaceEngineers.Core.AutoRegistration.Extensions
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using AutoWiring.Api.Abstractions;
    using AutoWiring.Api.Enumerations;
    using Basics;
    using Internals;
    using SimpleInjector;

    internal static class ContainerExtensions
    {
        internal static IEnumerable<Type> GetTypesToRegister(
            this Container container,
            Type serviceType,
            ITypeProvider typeProvider)
        {
            return container.GetTypesToRegister(
                serviceType,
                typeProvider.OurAssemblies.ToArray(),
                new TypesToRegisterOptions
                {
                    IncludeComposites = false,
                    IncludeDecorators = false,
                    IncludeGenericTypeDefinitions = true
                });
        }

        internal static IEnumerable<Type> RegisteredComponents(this Container container)
        {
            var explicitRegistrations = container
                .GetFieldValue<IDictionary>("explicitRegistrations")
                .Keys
                .OfType<Type>();

            var graph = container
                .GetCurrentRegistrations()
                .Select(DependencyInfo.RetrieveDependencyGraph)
                .SelectMany(info => info.ExtractFromGraph(dependency => dependency.ImplementationType));

            return explicitRegistrations
                .Concat(graph)
                .Distinct()
                .Where(type => !type.IsAbstract);
        }

        internal static bool IsOpenGenericFallback(this ServiceRegistrationInfo info)
        {
            return info.ImplementationType.IsGenericType
                   && !info.ImplementationType.IsConstructedGenericType;
        }

        internal static bool ForAutoRegistration(this ServiceRegistrationInfo info)
        {
            return ForAutoRegistration(info.RegistrationKind);
        }

        private static bool ForAutoRegistration(EnComponentRegistrationKind kind)
        {
            return kind == EnComponentRegistrationKind.AutomaticallyRegistered;
        }
    }
}