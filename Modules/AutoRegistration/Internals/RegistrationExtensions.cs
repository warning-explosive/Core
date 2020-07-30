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

    internal static class RegistrationExtensions
    {
        internal static bool ForAutoRegistration(this Type implementationType)
        {
            return implementationType.GetCustomAttribute<UnregisteredAttribute>(true) == null
                && implementationType.GetCustomAttribute<ManualRegistrationAttribute>(true) == null;
        }

        internal static bool IsNotVersion(this Type implementationType, ITypeExtensions typeExtensions)
        {
            if (typeExtensions.IsSubclassOfOpenGeneric(implementationType, typeof(IVersionFor<>)))
            {
                return typeExtensions
                      .GetGenericArgumentsOfOpenGenericAt(implementationType, typeof(IVersionFor<>), 0)
                      .All(serviceType => !serviceType.IsAssignableFrom(implementationType));
            }

            return true;
        }

        internal static IEnumerable<Type> GetTypesToRegister(this Type serviceType, Container container, ITypeExtensions typeExtensions)
        {
            return container.GetTypesToRegister(serviceType,
                                                typeExtensions.OurAssemblies(),
                                                new TypesToRegisterOptions
                                                {
                                                    IncludeComposites = false,
                                                    IncludeDecorators = false,
                                                    IncludeGenericTypeDefinitions = true
                                                });
        }

        internal static void RegisterDecorator(this Container container, ServiceRegistrationInfo info)
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

        internal static void RegisterWithOpenGenericFallBack(this Container container, IEnumerable<ServiceRegistrationInfo> infos)
        {
            infos.Select(info => new
                                 {
                                     Info = info,
                                     FallBackFor = info.ImplementationType.GetCustomAttribute<OpenGenericFallBackAttribute>()
                                 })
                 .OrderBy(pair => pair.FallBackFor != null) // fallback must be registered after all exact registrations
                 .Each(pair =>
                       {
                           if (pair.FallBackFor != null)
                           {
                               if (pair.FallBackFor.ServiceType == pair.Info.ServiceType)
                               {
                                   container.RegisterConditional(pair.Info.ServiceType,
                                                                 pair.Info.ImplementationType,
                                                                 pair.Info.Lifestyle,
                                                                 ctx => !ctx.Handled);
                               }
                           }
                           else
                           {
                               container.Register(pair.Info.ServiceType, pair.Info.ImplementationType, pair.Info.Lifestyle);
                           }
                       });
        }

        internal static void RegisterImplementations(this Container container, IEnumerable<ServiceRegistrationInfo> serviceRegistrationInfos)
        {
            foreach (var info in serviceRegistrationInfos)
            {
                container.Register(info.ImplementationType, info.ImplementationType, info.Lifestyle);
            }
        }

        internal static void RegisterCollections(this Container container, IEnumerable<ServiceRegistrationInfo> serviceRegistrationInfos)
        {
            serviceRegistrationInfos.OrderByDependencies(z => z.ImplementationType)
                                    .GroupBy(k => k.ServiceType, v => v.ImplementationType)
                                    .Each(info => container.Collection.Register(info.Key, info));
        }
    }
}