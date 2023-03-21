namespace SpaceEngineers.Core.CompositionRoot.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using Registration;
    using SimpleInjector;

    internal static class RegistrationExtensions
    {
        internal static void RegisterDecorators(this IEnumerable<DecoratorRegistrationInfo> infos, Container container)
        {
            infos
               .OrderByDependencies(info => info.Implementation)
               .Each(info => container.RegisterDecorator(info.Service, info.Implementation, info.Lifestyle.MapLifestyle()));
        }

        internal static void RegisterCollections(this IEnumerable<IRegistrationInfo> infos, Container container)
        {
            infos
               .GroupBy(info => info.Service)
               .Each(grp => RegisterCollection(
                    grp.OrderByDependencies(GetRegistrationKey),
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
                Container container)
            {
                infos.Each(info =>
                {
                    switch (info)
                    {
                        case InstanceRegistrationInfo instanceRegistrationInfo:
                            RegisterCollectionEntryInstance(container,
                                instanceRegistrationInfo.Service,
                                instanceRegistrationInfo.Instance);
                            break;
                        case ServiceRegistrationInfo serviceRegistrationInfo:
                            RegisterCollectionEntry(container,
                                serviceRegistrationInfo.Service,
                                serviceRegistrationInfo.Implementation,
                                serviceRegistrationInfo.Lifestyle);
                            break;
                        case DelegateRegistrationInfo delegateRegistrationInfo:
                            RegisterCollectionEntryDelegate(container,
                                delegateRegistrationInfo.Service,
                                delegateRegistrationInfo.InstanceProducer,
                                delegateRegistrationInfo.Lifestyle);
                            break;
                        default:
                            throw new NotSupportedException(info.GetType().Name);
                    }
                });
            }

            static void RegisterCollectionEntry(
                Container container,
                Type service,
                Type implementation,
                EnLifestyle lifestyle)
            {
                container.Collection.Append(service, implementation, lifestyle.MapLifestyle());
            }

            static void RegisterCollectionEntryInstance(
                Container container,
                Type service,
                object collectionEntryInstance)
            {
                container.Collection.AppendInstance(service, collectionEntryInstance);
            }

            static void RegisterCollectionEntryDelegate(
                Container container,
                Type service,
                Func<object> instanceProducer,
                EnLifestyle lifestyle)
            {
                container.AppendCollectionInstanceProducer(service, instanceProducer, lifestyle);
            }
        }

        internal static void RegisterInstances(
            this IEnumerable<InstanceRegistrationInfo> instances,
            Container container)
        {
            instances.Each(info => container.RegisterInstance(info.Service, info.Instance));
        }

        internal static void RegisterDelegates(this IEnumerable<DelegateRegistrationInfo> infos, Container container)
        {
            infos.Each(info => container.Register(info.Service, info.InstanceProducer, info.Lifestyle.MapLifestyle()));
        }

        internal static void RegisterResolvable(
            this IEnumerable<ServiceRegistrationInfo> infos,
            Container container)
        {
            infos.RegisterServicesWithOpenGenericFallBack(container);
        }

        private static void RegisterServicesWithOpenGenericFallBack(
            this IEnumerable<ServiceRegistrationInfo> infos,
            Container container)
        {
            RegisterWithOpenGenericFallBack(container, infos, info => info.Service);
        }

        private static void RegisterImplementationsWithOpenGenericFallBack(
            this IEnumerable<ServiceRegistrationInfo> infos,
            Container container)
        {
            RegisterWithOpenGenericFallBack(container, infos, info => info.Implementation);
        }

        private static void RegisterWithOpenGenericFallBack(
            this Container container,
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
                       container.Register(service, info.Implementation, info.Lifestyle.MapLifestyle());
                   }
               });
        }

        private static void RegisterOpenGenericFallBack(
            this Container container,
            Type service,
            Type implementation,
            EnLifestyle lifestyle)
        {
            container.RegisterConditional(service, implementation, lifestyle.MapLifestyle(), ctx => !ctx.Handled);
        }

        private static void AppendCollectionInstanceProducer(
            this Container container,
            Type service,
            Func<object> instanceProducer,
            EnLifestyle lifestyle)
        {
            typeof(RegistrationExtensions)
               .CallMethod(nameof(AppendCollectionInstanceProducer))
               .WithTypeArgument(service)
               .WithArguments(container, instanceProducer, lifestyle)
               .Invoke();
        }

        private static void AppendCollectionInstanceProducer<TService>(
            this Container container,
            Func<object> instanceProducer,
            EnLifestyle lifestyle)
            where TService : class
        {
            container.Collection.Append(() => (TService)instanceProducer(), lifestyle.MapLifestyle());
        }
    }
}