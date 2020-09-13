namespace SpaceEngineers.Core.AutoRegistration.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using AutoWiringApi.Abstractions;
    using AutoWiringApi.Attributes;
    using AutoWiringApi.Services;
    using Basics;
    using Internals;
    using SimpleInjector;

    internal static class RegistrationExtensions
    {
        internal static bool ForAutoRegistration(this Type implementationType)
        {
            return !implementationType.HasAttribute<UnregisteredAttribute>()
                && !implementationType.HasAttribute<ManualRegistrationAttribute>();
        }

        internal static bool IsVersion(this Type implementationType)
        {
            if (implementationType.IsSubclassOfOpenGeneric(typeof(IVersionFor<>)))
            {
                return implementationType.ExtractGenericArgumentsAt(typeof(IVersionFor<>), 0)
                                         .Any(serviceType => serviceType.IsAssignableFrom(implementationType));
            }

            return false;
        }

        internal static IEnumerable<Type> GetTypesToRegister(this Type serviceType, Container container, ITypeProvider typeProvider)
        {
            return container.GetTypesToRegister(serviceType,
                                                typeProvider.OurAssemblies,
                                                new TypesToRegisterOptions
                                                {
                                                    IncludeComposites = false,
                                                    IncludeDecorators = false,
                                                    IncludeGenericTypeDefinitions = true
                                                });
        }

        internal static void RegisterDecorator(this Container container, DecoratorRegistrationInfo info)
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
                                            c => c.ImplementationType.GetCustomAttribute(info.Attribute) != null);
            }
        }

        internal static void RegisterCollections(this Container container, ICollection<ServiceRegistrationInfo> infos)
        {
            // register each element of collection as implementation to provide lifestyle for container
            container.RegisterImplementationsWithOpenGenericFallBack(infos);

            infos.OrderByDependencyAttribute(z => z.ImplementationType)
                 .GroupBy(k => k.ServiceType, v => v.ImplementationType)
                 .Each(info => container.Collection.Register(info.Key, info));
        }

        internal static void RegisterVersionedComponents(this Container container, ICollection<ServiceRegistrationInfo> infos)
        {
            container.RegisterServicesWithOpenGenericFallBack(infos);
            container.RegisterImplementationsWithOpenGenericFallBack(infos);
        }

        internal static void RegisterServicesWithOpenGenericFallBack(this Container container, IEnumerable<ServiceRegistrationInfo> infos)
        {
            RegisterWithOpenGenericFallBack(container, infos, info => info.ServiceType);
        }

        internal static void RegisterImplementationsWithOpenGenericFallBack(this Container container, IEnumerable<ServiceRegistrationInfo> infos)
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
                                     FallBackFor = info.ImplementationType.GetCustomAttribute<OpenGenericFallBackAttribute>()
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