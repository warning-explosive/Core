namespace SpaceEngineers.Core.AutoRegistration.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AutoWiring.Api.Enumerations;
    using Basics;
    using Internals;
    using SimpleInjector;

    internal static class RegistrationExtensions
    {
        internal static void RegisterDecorators(this IEnumerable<DecoratorRegistrationInfo> infos, Container container)
        {
            infos.OrderByDependencyAttribute(z => z.ImplementationType)
                 .Each(info =>
                       {
                           if (info.ConditionAttribute == null)
                           {
                               container.RegisterDecorator(info.ServiceType,
                                                           info.ImplementationType,
                                                           info.Lifestyle);
                           }
                           else
                           {
                               container.RegisterDecorator(info.ServiceType,
                                                           info.ImplementationType,
                                                           info.Lifestyle,
                                                           c => c.ImplementationType.HasAttribute(info.ConditionAttribute));
                           }
                       });
        }

        internal static void RegisterCollections(this IReadOnlyCollection<ServiceRegistrationInfo> infos, Container container)
        {
            // register each element of collection as implementation to provide lifestyle for container
            infos.RegisterImplementationsWithOpenGenericFallBack(container);

            infos.OrderByDependencyAttribute(z => z.ImplementationType)
                 .GroupBy(k => k.ServiceType, v => v.ImplementationType)
                 .Each(info => container.Collection.Register(info.Key, info));
        }

        internal static void RegisterSingletons(this IEnumerable<(Type, object)> singletons, Container container)
        {
            foreach (var (service, instance) in singletons)
            {
                container.RegisterInstance(service, instance);
            }
        }

        internal static void RegisterServicesWithOpenGenericFallBack(this IEnumerable<ServiceRegistrationInfo> infos, Container container)
        {
            RegisterWithOpenGenericFallBack(container, infos, info => info.ServiceType);
        }

        internal static void RegisterImplementationsWithOpenGenericFallBack(this IEnumerable<ServiceRegistrationInfo> infos, Container container)
        {
            RegisterWithOpenGenericFallBack(container, infos, info => info.ImplementationType);
        }

        private static void RegisterWithOpenGenericFallBack(this Container container,
                                                            IEnumerable<ServiceRegistrationInfo> infos,
                                                            Func<ServiceRegistrationInfo, Type> serviceSelector)
        {
            infos // open-generic fallback should be registered after all exact registrations
                .OrderBy(info => info.ComponentKind == EnComponentKind.OpenGenericFallback)
                .Each(info =>
                {
                    var service = serviceSelector(info);

                    if (info.ComponentKind == EnComponentKind.OpenGenericFallback)
                    {
                        container.RegisterConditional(
                            service,
                            info.ImplementationType,
                            info.Lifestyle,
                            ctx => !ctx.Handled);
                    }
                    else
                    {
                        container.Register(
                            service,
                            info.ImplementationType,
                            info.Lifestyle);
                    }
                });
        }
    }
}