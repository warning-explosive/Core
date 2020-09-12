namespace SpaceEngineers.Core.AutoRegistration.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AutoWiringApi.Abstractions;
    using AutoWiringApi.Services;
    using Basics;
    using Implementations;
    using Internals;
    using SimpleInjector;

    internal static class ServiceRegistrationInfoExtensions
    {
        internal static IEnumerable<ServiceRegistrationInfo> VersionedComponents(this IEnumerable<Type> servicesWithVersions, Container container)
        {
            foreach (var serviceType in servicesWithVersions)
            {
                var versionedServiceType = typeof(IVersioned<>).MakeGenericType(serviceType);
                var versionedImplementationType = typeof(Versioned<>).MakeGenericType(serviceType);
                var lifestyle = container.GetRegistration(serviceType)
                                         .EnsureNotNull($"Container must has registration of {serviceType.FullName}")
                                         .Lifestyle.MapLifestyle();
                yield return new ServiceRegistrationInfo(versionedServiceType, versionedImplementationType, lifestyle);
            }
        }

        internal static IEnumerable<ServiceRegistrationInfo> GetComponents(this IEnumerable<Type> serviceTypes,
                                                                           Container container,
                                                                           ITypeProvider typeProvider,
                                                                           bool versions)
        {
            return serviceTypes.SelectMany(serviceType => serviceType.GetTypesToRegister(container, typeProvider)
                                                                     .Select(implementationType => (serviceType, implementationType)))
                               .GetComponents(versions);
        }

        private static IEnumerable<ServiceRegistrationInfo> GetComponents(this IEnumerable<(Type ServiceType, Type ImplementationType)> pairs, bool versions)
        {
            return pairs
                  .Where(pair => pair.ServiceType.ForAutoRegistration()
                                 && pair.ImplementationType.ForAutoRegistration())
                  .SelectMany(pair => GetClosedGenericImplForOpenGenericService(pair.ServiceType, pair.ImplementationType))
                  .Select(pair => new
                                  {
                                      pair.ServiceType,
                                      pair.ImplementationType,
                                      IsVersion = pair.ImplementationType.IsVersion()
                                  })
                  .Where(info => versions ? info.IsVersion : !info.IsVersion)
                  .Select(pair => new ServiceRegistrationInfo(pair.ServiceType, pair.ImplementationType, pair.ImplementationType.Lifestyle()));
        }

        private static IEnumerable<(Type ImplementationType, Type ServiceType)> GetClosedGenericImplForOpenGenericService(Type serviceType, Type componentType)
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