namespace SpaceEngineers.Core.AutoRegistration.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using AutoWiring.Api.Abstractions;
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Services;
    using Basics;
    using Internals;
    using SimpleInjector;

    internal static class RegistrationExtensions
    {
        internal static IEnumerable<Type> GetTypesToRegister(
            this Type serviceType,
            Container container,
            Assembly[] assemblies)
        {
            return container.GetTypesToRegister(serviceType,
                assemblies,
                new TypesToRegisterOptions
                {
                    IncludeComposites = false,
                    IncludeDecorators = false,
                    IncludeGenericTypeDefinitions = true
                });
        }

        internal static IEnumerable<Type> GetTypesToRegister(
            this Type serviceType,
            Container container,
            ITypeProvider typeProvider)
        {
            return serviceType.GetTypesToRegister(container, typeProvider.OurAssemblies.ToArray());
        }

        internal static bool ForAutoRegistration(this Type implementationType)
        {
            return !implementationType.HasAttribute<UnregisteredAttribute>()
                && !implementationType.HasAttribute<ManualRegistrationAttribute>();
        }

        internal static void RegisterDecorators(this IEnumerable<DecoratorRegistrationInfo> infos, Container container)
        {
            infos.OrderByDependencyAttribute(z => z.ImplementationType)
                 .Each(info =>
                       {
                           if (info.Attribute == null)
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
                                                           c => c.ImplementationType.HasAttribute(info.Attribute));
                           }
                       });
        }

        internal static void RegisterCollections(this ICollection<ServiceRegistrationInfo> infos, Container container)
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
            infos.Select(info => new
                                 {
                                     Info = info,
                                     FallBackFor = info.ImplementationType.GetAttribute<OpenGenericFallBackAttribute>()
                                 })
                 .OrderBy(pair => pair.FallBackFor != null) // fallback must be registered after all exact registrations
                 .Each(pair =>
                       {
                           var service = serviceSelector(pair.Info);

                           if (pair.FallBackFor != null)
                           {
                               container.RegisterConditional(service,
                                                             pair.Info.ImplementationType,
                                                             pair.Info.Lifestyle,
                                                             ctx => !ctx.Handled);
                           }
                           else
                           {
                               container.Register(service,
                                                  pair.Info.ImplementationType,
                                                  pair.Info.Lifestyle);
                           }
                       });
        }
    }
}