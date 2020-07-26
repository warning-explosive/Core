namespace SpaceEngineers.Core.AutoRegistration.Internals
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using AutoWiringApi.Attributes;
    using Basics;
    using SimpleInjector;

    internal static class RegistrationExtensions
    {
        internal static void RegisterDecorator(this Container container, ServiceRegistrationInfo info)
        {
            if (info.Attribute == null)
            {
                container.RegisterDecorator(info.ServiceType,
                                            info.ComponentType,
                                            info.Lifestyle);
            }
            else
            {
                container.RegisterDecorator(info.ServiceType,
                                            info.ComponentType,
                                            info.Lifestyle,
                                            c => c.ImplementationType.GetCustomAttribute(info.Attribute) != null);
            }
        }

        internal static void RegisterWithOpenGenericFallBack(this Container container, IEnumerable<ServiceRegistrationInfo> infos)
        {
            infos.Select(info => new
                                 {
                                     Info = info,
                                     FallBackFor = info.ComponentType.GetCustomAttribute<OpenGenericFallBackAttribute>()
                                 })
                 .OrderBy(pair => pair.FallBackFor != null) // fallback must be registered after all exact registrations
                 .Each(pair =>
                       {
                           if (pair.FallBackFor != null)
                           {
                               if (pair.FallBackFor.ServiceType == pair.Info.ServiceType)
                               {
                                   container.RegisterConditional(pair.Info.ServiceType,
                                                                 pair.Info.ComponentType,
                                                                 pair.Info.Lifestyle,
                                                                 ctx => !ctx.Handled);
                               }
                           }
                           else
                           {
                               container.Register(pair.Info.ServiceType, pair.Info.ComponentType, pair.Info.Lifestyle);
                           }
                       });
        }

        internal static void RegisterImplementations(this Container container, IEnumerable<ServiceRegistrationInfo> serviceRegistrationInfos)
        {
            foreach (var info in serviceRegistrationInfos)
            {
                container.Register(info.ComponentType, info.ComponentType, info.Lifestyle);
            }
        }

        internal static void RegisterCollections(this Container container, IEnumerable<ServiceRegistrationInfo> serviceRegistrationInfos)
        {
            serviceRegistrationInfos.OrderByDependencies(z => z.ComponentType)
                                    .GroupBy(k => k.ServiceType, v => v.ComponentType)
                                    .Each(info => container.Collection.Register(info.Key, info));
        }
    }
}