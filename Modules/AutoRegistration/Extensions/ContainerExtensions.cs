namespace SpaceEngineers.Core.AutoRegistration.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Services;
    using Basics;
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

        internal static bool ForAutoRegistration(this Type implementationType)
        {
            return !implementationType.HasAttribute<UnregisteredAttribute>()
                   && !implementationType.HasAttribute<ManualRegistrationAttribute>();
        }
    }
}