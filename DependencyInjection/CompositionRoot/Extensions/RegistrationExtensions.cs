namespace SpaceEngineers.Core.CompositionRoot.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Api.Abstractions;
    using Basics;
    using Registration;

    internal static class RegistrationExtensions
    {
        internal static void RegisterDecorators(this IEnumerable<DecoratorRegistrationInfo> infos, IDependencyContainerImplementation container)
        {
            infos.OrderByDependencyAttribute(info => info.Implementation)
                 .Each(info =>
                       {
                           container.RegisterDecorator(info.Service, info.Implementation, info.Lifestyle);
                       });
        }

        internal static void RegisterCollections(this IEnumerable<ServiceRegistrationInfo> infos, IDependencyContainerImplementation container)
        {
            infos
                .GroupBy(info => new { info.Service, info.Lifestyle }, info => info.Implementation)
                .Each(grp => container.RegisterCollection(grp.Key.Service, grp, grp.Key.Lifestyle));
        }

        internal static void RegisterInstances(this IEnumerable<InstanceRegistrationInfo> instances, IDependencyContainerImplementation container)
        {
            foreach (var info in instances)
            {
                container.RegisterInstance(info.Service, info.Instance);
            }
        }

        internal static void RegisterDelegates(this IEnumerable<DelegateRegistrationInfo> infos, IDependencyContainerImplementation container)
        {
            foreach (var info in infos)
            {
                container.Register(info.Service, info.InstanceProducer, info.Lifestyle);
            }
        }

        internal static void RegisterResolvable(this IEnumerable<ServiceRegistrationInfo> infos, IDependencyContainerImplementation container)
        {
            infos.RegisterServicesWithOpenGenericFallBack(container);
        }

        internal static void RegisterServicesWithOpenGenericFallBack(this IEnumerable<ServiceRegistrationInfo> infos, IDependencyContainerImplementation container)
        {
            RegisterWithOpenGenericFallBack(container, infos, info => info.Service);
        }

        internal static void RegisterImplementationsWithOpenGenericFallBack(this IEnumerable<ServiceRegistrationInfo> infos, IDependencyContainerImplementation container)
        {
            RegisterWithOpenGenericFallBack(container, infos, info => info.Implementation);
        }

        private static void RegisterWithOpenGenericFallBack(this IDependencyContainerImplementation container,
                                                            IEnumerable<ServiceRegistrationInfo> infos,
                                                            Func<ServiceRegistrationInfo, Type> serviceSelector)
        {
            infos // open-generic fallback should be registered after all exactly registered components
                .OrderBy(info => info.IsOpenGenericFallback())
                .Each(info =>
                {
                    var service = serviceSelector(info);

                    if (info.IsOpenGenericFallback())
                    {
                        container.RegisterOpenGenericFallBack(service, info.Implementation, info.Lifestyle);
                    }
                    else
                    {
                        container.Register(service, info.Implementation, info.Lifestyle);
                    }
                });
        }
    }
}