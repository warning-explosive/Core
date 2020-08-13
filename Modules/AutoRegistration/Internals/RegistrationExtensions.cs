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
            serviceRegistrationInfos.OrderByDependencyAttribute(z => z.ImplementationType)
                                    .GroupBy(k => k.ServiceType, v => v.ImplementationType)
                                    .Each(info => container.Collection.Register(info.Key, info));
        }
    }
}