namespace SpaceEngineers.Core.AutoRegistration.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AutoWiring.Api.Services;
    using Basics;
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
                .SelectMany(serviceType => container
                    .GetTypesToRegister(serviceType, typeProvider)
                    .Select(implementationType => (serviceType, implementationType)))
                .GetComponents()
                .Distinct();
        }

        private static IEnumerable<ServiceRegistrationInfo> GetComponents(
            this IEnumerable<(Type ServiceType, Type ImplementationType)> infos)
        {
            return infos
                .SelectMany(info => GetClosedGenericImplForOpenGenericService(info.ServiceType, info.ImplementationType)
                    .Select(innerInfo => (innerInfo.ServiceType, innerInfo.ImplementationType)))
                .Select(info => new ServiceRegistrationInfo(info.ServiceType, info.ImplementationType))
                .Where(info => info.ForAutoRegistration());
        }

        private static IEnumerable<(Type ImplementationType, Type ServiceType)> GetClosedGenericImplForOpenGenericService(
            Type serviceType,
            Type implementationType)
        {
            if (serviceType.IsGenericType
             && serviceType.IsGenericTypeDefinition
             && !implementationType.IsGenericType)
            {
                var length = serviceType.GetGenericArguments().Length;

                var argsColumnStore = new Dictionary<int, ICollection<Type>>(length);

                for (var i = 0; i < length; ++i)
                {
                    argsColumnStore[i] = implementationType.ExtractGenericArgumentsAt(serviceType, i).ToList();
                }

                foreach (var typeArguments in argsColumnStore.Values.ColumnsCartesianProduct())
                {
                    var closedService = serviceType.MakeGenericType(typeArguments.ToArray());
                    yield return (implementationType, closedService);
                }
            }
            else
            {
                yield return (implementationType, serviceType);
            }
        }
    }
}