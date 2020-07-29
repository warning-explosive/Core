namespace SpaceEngineers.Core.AutoRegistration.Internals
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using AutoWiringApi.Abstractions;
    using AutoWiringApi.Attributes;
    using Basics;
    using SimpleInjector;

    internal static class ServiceRegistrationInfoExtensions
    {
        internal static IEnumerable<ServiceRegistrationInfo> VersionedComponents(this IEnumerable<Type> servicesWithVersions, Container container)
        {
            foreach (var serviceType in servicesWithVersions)
            {
                var versionedServiceType = typeof(IVersioned<>).MakeGenericType(serviceType);
                var versionedImplementationType = typeof(Versioned<>).MakeGenericType(serviceType);
                var lifestyle = container.GetRegistration(serviceType).Lifestyle.MapLifestyle();
                yield return new ServiceRegistrationInfo(versionedServiceType, versionedImplementationType, lifestyle);
            }
        }

        internal static IEnumerable<ServiceRegistrationInfo> GetComponents(this IEnumerable<Type> serviceTypes,
                                                                           Container container,
                                                                           ITypeExtensions typeExtensions,
                                                                           bool withVersions)
        {
            return serviceTypes.SelectMany(serviceType => serviceType.GetTypesToRegister(container, typeExtensions)
                                                                     .Select(implementationType => (serviceType, implementationType)))
                               .GetComponents(typeExtensions, withVersions);
        }

        internal static IEnumerable<ServiceRegistrationInfo> GetImplementationComponents(this IEnumerable<Type> implementationTypes, ITypeExtensions typeExtensions, bool withVersions)
        {
            return implementationTypes.Select(implementationType => (implementationType, implementationType)).GetComponents(typeExtensions, withVersions);
        }

        private static IEnumerable<ServiceRegistrationInfo> GetComponents(this IEnumerable<(Type ServiceType, Type ImplementationType)> pairs, ITypeExtensions typeExtensions, bool withVersions)
        {
            return pairs
                  .Where(pair => pair.ImplementationType.ForAutoRegistration())
                  .SelectMany(pair => GetClosedGenericImplForOpenGenericService(pair.ServiceType, pair.ImplementationType))
                  .Where(pair => withVersions || pair.ImplementationType.IsNotVersion(typeExtensions))
                  .Select(pair => new ServiceRegistrationInfo(pair.ServiceType, pair.ImplementationType, pair.ImplementationType.GetCustomAttribute<LifestyleAttribute>()?.Lifestyle));
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
                    argsColumnStore[i] = componentType.GetGenericArgumentsOfOpenGenericAt(serviceType, i).ToList();
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