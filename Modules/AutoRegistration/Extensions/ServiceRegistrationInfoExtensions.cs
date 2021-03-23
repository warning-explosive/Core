namespace SpaceEngineers.Core.AutoRegistration.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AutoWiring.Api.Abstractions;
    using AutoWiring.Api.Enumerations;
    using AutoWiring.Api.Services;
    using Basics;
    using Implementations;
    using Internals;
    using SimpleInjector;

    internal static class ServiceRegistrationInfoExtensions
    {
        internal static IEnumerable<ServiceRegistrationInfo> GetComponents(
            this IEnumerable<Type> serviceTypes,
            Container container,
            ITypeProvider typeProvider)
        {
            return serviceTypes
                .SelectMany(serviceType => serviceType
                    .GetTypesToRegister(container, typeProvider)
                    .Select(implementationType => (serviceType, implementationType)))
                .GetComponents()
                .Distinct();
        }

        private static IEnumerable<ServiceRegistrationInfo> GetComponents(
            this IEnumerable<(Type ServiceType, Type ImplementationType)> pairs)
        {
            return pairs
                  .Where(pair => pair.ServiceType.ForAutoRegistration()
                                 && pair.ImplementationType.ForAutoRegistration())
                  .SelectMany(pair => GetClosedGenericImplForOpenGenericService(pair.ServiceType, pair.ImplementationType))
                  .Select(pair => new
                                  {
                                      pair.ServiceType,
                                      pair.ImplementationType
                                  })
                  .Select(pair => new ServiceRegistrationInfo(pair.ServiceType, pair.ImplementationType, pair.ImplementationType.Lifestyle()));
        }

        private static IEnumerable<(Type ImplementationType, Type ServiceType)> GetClosedGenericImplForOpenGenericService(
            Type serviceType,
            Type componentType)
        {
            if (serviceType.IsGenericType
             && serviceType.IsGenericTypeDefinition
             && !componentType.IsGenericType)
            {
                var length = serviceType.GetGenericArguments().Length;

                var argsColumnStore = new Dictionary<int, ICollection<Type>>(length);

                for (var i = 0; i < length; ++i)
                {
                    argsColumnStore[i] = componentType.ExtractGenericArgumentsAt(serviceType, i).ToList();
                }

                foreach (var typeArguments in argsColumnStore.Values.ColumnsCartesianProduct())
                {
                    var closedService = serviceType.MakeGenericType(typeArguments.ToArray());
                    yield return (componentType, closedService);
                }
            }
            else
            {
                yield return (componentType, serviceType);
            }
        }
    }
}