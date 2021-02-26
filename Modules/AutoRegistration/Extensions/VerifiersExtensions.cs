namespace SpaceEngineers.Core.AutoRegistration.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Basics;
    using SimpleInjector;

    internal static class VerifiersExtensions
    {
        internal static IEnumerable<Type> RegisteredComponents(this Container container)
        {
            return container
                .GetCurrentRegistrations()
                .Select(producer => producer.Registration.ImplementationType)
                .Where(type => !type.IsInterface && !type.IsAbstract)
                .Select(type => type.UnwrapTypeParameter(typeof(IEnumerable<>)))
                .Distinct();
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
    }
}