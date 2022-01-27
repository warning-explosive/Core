namespace SpaceEngineers.Core.CompositionRoot.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Api.Abstractions.Container;
    using Api.Abstractions.Registration;
    using Basics;

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

        internal static void RegisterCollections(this IEnumerable<IRegistrationInfo> infos, IDependencyContainerImplementation container)
        {
            infos
                .GroupBy(info => info.Service)
                .Each(grp => RegisterCollection(
                    grp.OrderByDependencyAttribute(GetRegistrationKey),
                    container));

            static Type GetRegistrationKey(IRegistrationInfo info)
            {
                return info switch
                {
                    InstanceRegistrationInfo instanceRegistrationInfo => instanceRegistrationInfo.Instance.GetType(),
                    ServiceRegistrationInfo serviceRegistrationInfo => serviceRegistrationInfo.Implementation,
                    DelegateRegistrationInfo delegateRegistrationInfo => typeof(object),
                    _ => throw new NotSupportedException(info.GetType().Name)
                };
            }

            static void RegisterCollection(
                IOrderedEnumerable<IRegistrationInfo> infos,
                IDependencyContainerImplementation container)
            {
                infos.Each(info =>
                {
                    switch (info)
                    {
                        case InstanceRegistrationInfo instanceRegistrationInfo:
                            container.RegisterCollectionEntryInstance(
                                instanceRegistrationInfo.Service,
                                instanceRegistrationInfo.Instance);
                            break;
                        case ServiceRegistrationInfo serviceRegistrationInfo:
                            container.RegisterCollectionEntry(
                                serviceRegistrationInfo.Service,
                                serviceRegistrationInfo.Implementation,
                                serviceRegistrationInfo.Lifestyle);
                            break;
                        case DelegateRegistrationInfo delegateRegistrationInfo:
                            container.RegisterCollectionEntryDelegate(
                                delegateRegistrationInfo.Service,
                                delegateRegistrationInfo.InstanceProducer,
                                delegateRegistrationInfo.Lifestyle);
                            break;
                        default:
                            throw new NotSupportedException(info.GetType().Name);
                    }
                });
            }
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
                container.RegisterDelegate(info.Service, info.InstanceProducer, info.Lifestyle);
            }
        }

        internal static void RegisterResolvable(
            this IEnumerable<ServiceRegistrationInfo> infos,
            IDependencyContainerImplementation container)
        {
            infos.RegisterServicesWithOpenGenericFallBack(container);
        }

        internal static void RegisterServicesWithOpenGenericFallBack(
            this IEnumerable<ServiceRegistrationInfo> infos,
            IDependencyContainerImplementation container)
        {
            RegisterWithOpenGenericFallBack(container, infos, info => info.Service);
        }

        internal static void RegisterImplementationsWithOpenGenericFallBack(
            this IEnumerable<ServiceRegistrationInfo> infos,
            IDependencyContainerImplementation container)
        {
            RegisterWithOpenGenericFallBack(container, infos, info => info.Implementation);
        }

        private static void RegisterWithOpenGenericFallBack(
            this IDependencyContainerImplementation container,
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