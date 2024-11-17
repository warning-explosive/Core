namespace SpaceEngineers.Core.DependencyInjection.Extensions;

using System;
using System.Collections.Generic;
using System.Linq;
using SimpleInjector;
using Basics;
using Registration;

internal static class SimpleInjectorRegistrationExtensions
{
    internal static void Register(
        this Container container,
        IEnumerable<IDependencyInjectionDependencyRegistrationInfo> infos)
    {
        // open-generic fallback should be registered after all exactly registered components
        foreach (var info in infos
                     .GroupBy(info => info.Service)
                     .Where(info => info.Count() == 1)
                     .Select(info => info.First())
                     .OrderBy(info => info is ServiceDependencyInjectionRegistrationInfo)
                     .ThenByDescending(info => info is not ServiceDependencyInjectionRegistrationInfo serviceDependencyInjectionRegistrationInfo
                                               || serviceDependencyInjectionRegistrationInfo.IsOpenGenericFallback()))
        {
            switch (info)
            {
                case InstanceDependencyInjectionRegistrationInfo instanceRegistrationInfo:
                    RegisterInstance(container, instanceRegistrationInfo);
                    break;

                case ServiceDependencyInjectionRegistrationInfo serviceRegistrationInfo:
                    RegisterService(container, serviceRegistrationInfo);
                    break;

                case DelegateDependencyInjectionRegistrationInfo delegateRegistrationInfo:
                    RegisterDelegate(container, delegateRegistrationInfo);
                    break;

                default:
                    throw new NotSupportedException(info.GetType().Name);
            }
        }

        static void RegisterInstance(
            Container container,
            InstanceDependencyInjectionRegistrationInfo info)
        {
            container.RegisterInstance(info.Service, info.Instance);
        }

        static void RegisterService(
            Container container,
            ServiceDependencyInjectionRegistrationInfo info)
        {
            if (info.IsOpenGenericFallback())
            {
                RegisterOpenGenericFallBack(container, info.Service, info.Implementation, info.Lifestyle);
            }
            else
            {
                container.Register(info.Service, info.Implementation, info.Lifestyle.MapLifestyle());
            }

            static void RegisterOpenGenericFallBack(
                Container container,
                Type service,
                Type implementation,
                EnLifestyle lifestyle)
            {
                container.RegisterConditional(
                    service,
                    implementation,
                    lifestyle.MapLifestyle(),
                    ctx => !ctx.Handled);
            }
        }

        static void RegisterDelegate(
            Container container,
            DelegateDependencyInjectionRegistrationInfo info)
        {
            container.Register(info.Service, info.InstanceProducer, info.Lifestyle.MapLifestyle());
        }
    }

    internal static void RegisterCollections(
        this Container container,
        IEnumerable<IDependencyInjectionRegistrationInfo> infos)
    {
        infos
            .GroupBy(info => info.Service)
            .Each(grp => RegisterCollection(grp.OrderByBeforeAfterAttributes(GetRegistrationKey), container));

        static Type GetRegistrationKey(IDependencyInjectionRegistrationInfo info)
        {
            return info switch
            {
                InstanceDependencyInjectionRegistrationInfo instanceRegistrationInfo => instanceRegistrationInfo.Instance.GetType(),
                ServiceDependencyInjectionRegistrationInfo serviceRegistrationInfo => serviceRegistrationInfo.Implementation,
                DelegateDependencyInjectionRegistrationInfo delegateRegistrationInfo => typeof(object),
                _ => throw new NotSupportedException(info.GetType().Name)
            };
        }

        static void RegisterCollection(
            IOrderedEnumerable<IDependencyInjectionRegistrationInfo> infos,
            Container container)
        {
            infos.Each(info =>
            {
                switch (info)
                {
                    case InstanceDependencyInjectionRegistrationInfo instanceRegistrationInfo:
                        RegisterCollectionEntryInstance(
                            container,
                            instanceRegistrationInfo.Service,
                            instanceRegistrationInfo.Instance);
                        break;
                    case ServiceDependencyInjectionRegistrationInfo serviceRegistrationInfo:
                        RegisterCollectionEntry(
                            container,
                            serviceRegistrationInfo.Service,
                            serviceRegistrationInfo.Implementation,
                            serviceRegistrationInfo.Lifestyle);
                        break;
                    case DelegateDependencyInjectionRegistrationInfo delegateRegistrationInfo:
                        RegisterCollectionEntryDelegate(
                            container,
                            delegateRegistrationInfo.Service,
                            delegateRegistrationInfo.InstanceProducer,
                            delegateRegistrationInfo.Lifestyle);
                        break;
                    default:
                        throw new NotSupportedException(info.GetType().Name);
                }
            });
        }

        static void RegisterCollectionEntryInstance(
            Container container,
            Type service,
            object collectionEntryInstance)
        {
            container.Collection.AppendInstance(service, collectionEntryInstance);
        }

        static void RegisterCollectionEntry(
            Container container,
            Type service,
            Type implementation,
            EnLifestyle lifestyle)
        {
            container.Collection.Append(service, implementation, lifestyle.MapLifestyle());
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

    internal static void RegisterDecorators(
        this Container container,
        IEnumerable<DecoratorDependencyInjectionRegistrationInfo> infos)
    {
        infos
            .OrderByBeforeAfterAttributes(info => info.Implementation)
            .Each(info => container.RegisterDecorator(info.Service, info.Implementation, info.Lifestyle.MapLifestyle()));
    }

    private static void AppendCollectionInstanceProducer(
        this Container container,
        Type service,
        Func<object> instanceProducer,
        EnLifestyle lifestyle)
    {
        typeof(SimpleInjectorRegistrationExtensions)
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