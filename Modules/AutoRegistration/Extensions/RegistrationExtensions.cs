namespace SpaceEngineers.Core.AutoRegistration.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
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

        internal static void RegisterCollections(this IEnumerable<ServiceRegistrationInfo> infos, Container container)
        {
            /*
             * Register each element of collection as implementation to provide lifestyle for container
             */

            var materialized = infos.ToList();

            materialized.RegisterImplementationsWithOpenGenericFallBack(container);

            materialized.OrderByDependencyAttribute(z => z.ImplementationType)
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

        internal static void RegisterDelegates(this IEnumerable<DelegateRegistrationInfo> infos, Container container)
        {
            foreach (var info in infos)
            {
                container.Register(info.ServiceType, info.Factory, info.Lifestyle);
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
            infos // open-generic fallback should be registered after all exactly registered components
                .OrderBy(info => info.IsOpenGenericFallback())
                .Each(info =>
                {
                    var service = serviceSelector(info);

                    if (info.IsOpenGenericFallback())
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