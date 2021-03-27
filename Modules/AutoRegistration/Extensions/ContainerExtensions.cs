namespace SpaceEngineers.Core.AutoRegistration.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AutoWiring.Api.Enumerations;
    using AutoWiring.Api.Services;
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

        internal static bool ForAutoRegistration(this ServiceRegistrationInfo info)
        {
            return ForAutoRegistration(info.ComponentKind);
        }

        private static bool ForAutoRegistration(EnComponentKind kind)
        {
            return kind == EnComponentKind.Regular
                   || kind == EnComponentKind.OpenGenericFallback;
        }
    }
}